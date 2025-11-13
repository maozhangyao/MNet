using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace MNet.SqlExpression
{
    /// <summary>
    /// 通用的生成sql的表达式翻译器
    /// </summary>
    public class CommonSqlExpressionBuilder : ExpressionVisitor
    {
        public CommonSqlExpressionBuilder()
        {
            this.Stack = new Stack<SqlToken>();
            this.Paramters = new List<SqlParamter>();
        }


        private DbType _dbType => this.Context?.Options?.Db ?? DbType.Mysql;


        public SqlBuildContext Context { get; set; }
        //当前生成过程中，产生的参数
        public List<SqlParamter> Paramters { get; set; }
        protected Stack<SqlToken> Stack { get; }


        protected SqlToken PopToken()
        {
            return this.Stack.Pop();
        }
        protected void PushToken(SqlToken token)
        {
            this.Stack.Push(token);
        }
        protected SqlToken PushToken(string part, object dynamic)
        {
            SqlToken token = new SqlToken(part, dynamic);
            this.PushToken(token);
            return token;
        }
        //增加一个SQL参数token
        protected SqlToken PushParameter(object val)
        {
            SqlParamter p = this.AddParameter(val);
            SqlToken token = new SqlToken(p.Name, val);
            this.PushToken(token);
            return token;
        }
        //增加一个SQL参数
        protected SqlParamter AddParameter(object val)
        {
            SqlParamter p = this.ToParameter(val);

            this.Paramters.Add(p);
            this.Context?.RefParameters?.Add(p);
            return p;
        }


        protected override Expression VisitConstant(ConstantExpression node)
        {
            base.VisitConstant(node);

            //访问到常量，该常量值需要动态求值
            this.PushToken(null, node.Value);
            return node;
        }
        protected override Expression VisitMember(MemberExpression node)
        {
            base.VisitMember(node);

            //访问参数的成员，一般为字段或者属性
            if (node.Expression is ParameterExpression parameter)
            {
                this.PushToken(DbUtils.Escape(node.Member.Name, this._dbType), null);
            }
            //访问到了静态成员的属性或者字段
            else if (node.Expression == null)
            {
                object val = this.TakeMemberValue(null, node.Member);
                this.PushToken(null, val);
            }
            //访问的实例成员的属性或者字段
            else
            {
                SqlToken pre = this.PopToken();
                if (pre.IsDynamic)
                {
                    //对常量分支做求值
                    object val = this.TakeMemberValue(pre.Dynamic, node.Member);
                    this.PushToken(null, val);
                }
                else
                {
                    //对参数分支做转换
                    string key = FunctionMapping.GetFunctionMapKey(node.Member);
                    string par = FunctionMapping.FunctionMaps[key](this._dbType, node.Member, new SqlToken[] { pre });
                    this.PushToken(par, null);
                }
            }
            return node;
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            base.VisitMethodCall(node);

            int args = node.Arguments.Count;
            Stack<SqlToken> sqlTokens = new Stack<SqlToken>();
            //调用了静态函数
            if (node.Object == null)
            {
                for (int i = 0; i < args; i++)
                    sqlTokens.Push(this.PopToken());
            }
            //调用了实例函数
            else
            {
                for (int i = 0; i <= args; i++)
                    sqlTokens.Push(this.PopToken());
            }

            SqlToken[] tokens = sqlTokens.ToArray();
            //如果所有的节点都需要动态求值，则
            if (sqlTokens.All(p => p.IsDynamic) //表示所有的参数节点都是需要动态求值的，所以直接动态求值 
                || (sqlTokens.Count <= 0 && node.Object == null)) //表示调用了无参数的静态函数，直接求值即可
            {
                //这个方法是应该动态求值，还是转化为sql
                object obj = node.Object == null ? node.Method.Invoke(null, tokens.Select(p => p.Dynamic).ToArray()) //调用静态函数
                    : node.Method.Invoke(tokens[0], tokens.Skip(1).Select(p => p.Dynamic).ToArray()); //调用实例函数

                this.PushToken(null, obj);
            }
            else
            {
                //直接转换为sql
                string key = FunctionMapping.GetFunctionMapKey(node.Method);
                string par = FunctionMapping.FunctionMaps[key](this._dbType, node.Method, sqlTokens.ToArray());
                this.PushToken(par, null);
            }

            return node;
        }
        protected override Expression VisitBinary(BinaryExpression node)
        {
            base.VisitBinary(node);

            string opt = (node.NodeType) switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "!=",
                ExpressionType.LessThan => "<",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThanOrEqual => ">=",
                ExpressionType.AndAlso => "and",
                ExpressionType.OrElse => "or",
                ExpressionType.Add => "+",
                _ => throw new NotImplementedException($"操作符{node.NodeType}未实现")
            };

            SqlToken right = this.PopToken();
            SqlToken left = this.PopToken();
            if (right.IsDynamic)
                right.SqlPart = this.ToParameter(right.Dynamic).Name;
            if (left.IsDynamic)
                left.SqlPart = this.ToParameter(left.Dynamic).Name;

            this.PushToken($"({left.SqlPart} {opt} {right.SqlPart})", null);
            //Console.WriteLine(node);
            return node;
        }
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            base.VisitLambda(node);

            SqlToken token = this.PopToken();
            if (token.IsDynamic)
                token.SqlPart = this.AddParameter(token.Dynamic).Name;

            this.PushToken(token);
            return node;
        }


        public SqlParamter ToParameter(object val)
        {
            Type type = val.GetType();
            //            bool flag = val == null || val is IEnumerable || type.IsPrimitive || val is string || val is Guid || val is DateTime || val is DateTimeOffset
            //#if !NETSTANDARD
            //                || val is DateOnly
            //#endif
            //                ;

            //            //复杂对象，无法参数化的，直接默认ToString, 表示已字符
            //            if (!flag)
            //               val = val.ToString();

            string pNames = null;
            if (val is IEnumerable enumer && !(val is string))
            {
                //列表
                foreach (var item in enumer)
                {
                    //this.ToParameter(item);
                    if (pNames == null)
                        pNames = $"@p{this.Context.RefParamCount++}";
                    else
                        pNames = pNames + "," + $"@p{this.Context.RefParamCount++}";
                }
            }
            else
            {
                pNames = $"@p{this.Context.RefParamCount++}";
            }
            return new SqlParamter(pNames, val);
        }
        /// <summary>
        /// 取出成员值
        /// </summary>
        /// <param name="inst"></param>
        /// <param name="member">必须是属性或者字段</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public object TakeMemberValue(object inst, MemberInfo member)
        {
            object val = null;
            if (member is FieldInfo fld)
                val = fld.GetValue(inst);
            else if (member is PropertyInfo pro)
                val = pro.GetValue(inst);
            else
                throw new Exception($"成员无法解析取值:{member.Name}, 类：{member.ReflectedType?.FullName}");

            return val;
        }
    }
}
