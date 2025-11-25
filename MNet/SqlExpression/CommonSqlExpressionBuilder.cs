using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Xml.Linq;

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

            //动态求值模式
            this._dvModel = new Stack<bool>();
        }


        //动态求值模式栈
        private Stack<bool> _dvModel;
        private DbType _dbType => this.Context?.Options?.Db ?? DbType.Mysql;


        public SqlBuildContext Context { get; set; }
        public SqlDescriptor SqlDecriptor { get; set; }
        //当前生成过程中，产生的参数
        public List<SqlParamter> Paramters { get; set; }
        //生成过程中的SQL token 栈
        protected Stack<SqlToken> Stack { get; }


        //以动态求值的模式开启访问
        private Expression VisitWithDvModel(Expression expr, bool dvModel)
        {
            this.PushDvModel(dvModel);
            expr = this.Visit(expr);
            this.PopDvModel();

            return expr;
        }
        //队成员访问做SQL转换
        private SqlToken DoMemberMapping(SqlToken inst, MemberExpression node)
        {
            MemberMappingContext mapp = new MemberMappingContext();
            mapp.SqlBuilder = new SqlBuilder();
            mapp.SqlOptions = this.Context.Options;
            mapp.Expr = node;
            mapp.MappMember = node.Member;
            mapp.Instance = inst;
            mapp.Args = Array.Empty<SqlToken>();
            
            string key = FunctionMapping.GetFunctionMapKey(node.Member);
            SqlToken token = FunctionMapping.FunctionMaps[key](mapp);
            return token;
        }
        private SqlToken DoMethodMapping(SqlToken inst, SqlToken[] args, MethodCallExpression node)
        {
            MemberMappingContext mapp = new MemberMappingContext();
            mapp.SqlBuilder = new SqlBuilder();
            mapp.BuildContext = this.Context;
            mapp.SqlOptions = this.Context.Options;
            mapp.Expr = node;
            mapp.MappMember = node.Method;
            mapp.Instance = inst;
            mapp.Args = args;
            
            //自定义转化
            string key = FunctionMapping.GetFunctionMapKey(node.Method);
            SqlToken token = FunctionMapping.FunctionMaps[key](mapp);
            return token;
        }


        protected bool IsDvModel()
        {
            return this._dvModel.Count > 0 ? this._dvModel.Peek() : false;
        }
        protected void PushDvModel(bool dvModel)
        {
            this._dvModel.Push(dvModel);
        }
        protected bool PopDvModel()
        {
            return this._dvModel.Pop();
        }
        protected SqlToken PeekToken()
        {
            return this.Stack.Count <= 0 ? null : this.Stack.Peek();
        }
        protected SqlToken PopToken()
        {
            return this.Stack.Pop();
        }
        //弹出指定的个数
        protected SqlToken[] PopToken(int cnt)
        {
            Stack<SqlToken> buffer = new Stack<SqlToken>(4);
            while (cnt-- > 0)
                buffer.Push(this.PopToken());
            return buffer.ToArray();
        }
        protected void PushToken(SqlToken token)
        {
            this.Stack.Push(token);
        }
        protected SqlToken PushToken(string part, object dynamic, Expression expr)
        {
            SqlToken token = new SqlToken(part, dynamic, expr);
            this.PushToken(token);
            return token;
        }
        //增加一个SQL参数token
        protected SqlToken PushParameter(object val, Expression parameter)
        {
            SqlParamter p = this.AddParameter(val);
            SqlToken token = new SqlToken(p.Name, val, parameter);
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


        protected override Expression VisitParameter(ParameterExpression node)
        {
            //绑定表格引用
            SqlScope scope = this.Context.SqlScope;
            while (scope != null && !scope.IsScope(node))
            {
                scope = scope.ParentScope;
            }
            if (scope == null)
                throw new Exception($"表达式参数：{node}，未找到作用域");

            //确定表名, 目前默认是 from 部分的表名
            string tname = this.SqlDecriptor.From.Name;
            this.PushToken(tname, null, node);

            return base.VisitParameter(node);
        }
        protected override Expression VisitConstant(ConstantExpression node)
        {
            base.VisitConstant(node);

            //访问到常量，该常量值需要动态求值
            this.PushToken(null, node.Value, node);
            return node;
        }
        protected override Expression VisitMember(MemberExpression node)
        {
             base.VisitMember(node);

            //访问参数的成员，一般为字段或者属性
            if (node.Expression is ParameterExpression parameter)
            {
                SqlToken pre = this.PopToken(); //表名成

                this.PushToken(pre.SqlPart + "." + DbUtils.Escape(node.Member.Name, this._dbType), null, node);
            }
            //访问到了静态成员的属性或者字段
            else if (node.Expression == null)
            {
                object val = this.TakeMemberValue(null, node.Member);
                this.PushToken(null, val, node);
            }
            //访问的实例成员的属性或者字段
            else
            {
                SqlToken pre = this.PopToken();
                string key = FunctionMapping.GetFunctionMapKey(node.Member);

                //自定义取值
                if (FunctionMapping.FunctionMaps.ContainsKey(key))
                {
                    SqlToken token = this.DoMemberMapping(pre, node);
                    this.PushToken(token);
                }
                //对常量分支做求值
                else if (pre.IsDynamic)
                {
                    object val = this.TakeMemberValue(pre.Dynamic, node.Member);
                    this.PushToken(null, val, node);
                }
                else
                {
                    throw new Exception($"无法转换该成员访问：{node}");
                }
            }
            return node;
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            //可能需求动态求值，也可能需要直接转换成SQL
            SqlToken obj = null;
            //是否静态方法
            bool isStatic = node.Object == null; 
            bool dynamic = this.IsDvModel() || isStatic;

            //首先访问实例对象，求出实例对象值
            if (node.Object != null)
            {
                this.Visit(node.Object);
                obj = this.PeekToken();
                dynamic |= obj.IsDynamic;
            }

            //访问参数
            foreach (Expression parameter in node.Arguments)
                this.VisitWithDvModel(parameter, dynamic);
            
            //取出参数
            SqlToken[] args = this.PopToken(node.Arguments.Count);
            //表示，参数中全部都是动态值，可以直接求值
            if (args.Length > 0)
                dynamic |= args.All(p => p.IsDynamic);

            string key = FunctionMapping.GetFunctionMapKey(node.Method);
            //自定义求值
            if (FunctionMapping.FunctionMaps.ContainsKey(key))
            {
                SqlToken token = this.DoMethodMapping(obj, args, node);
                this.PushToken(token);
            }
            //动态求值
            else if (dynamic)
            {
                object inst = obj != null ? obj.Dynamic : null;
                object val = node.Method.Invoke(inst, args.Select(p => p.Dynamic).ToArray());

                this.PushToken(null, val, node);
            }
            //无法求值
            else
            {
                throw new Exception($"无法转换该方法调用：{node}");
            }

            return node;
        }
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (this.IsDvModel())
            {
                //本身就是值(表示一个函数实例对象)
                this.PushToken(null, node, node); 
                return node;
            }

            base.VisitLambda(node);
            
            SqlToken token = this.PopToken();
            if (token.IsDynamic)
                token.SqlPart = this.AddParameter(token.Dynamic).Name;

            this.PushToken(token);
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

            this.PushToken($"({left.SqlPart} {opt} {right.SqlPart})", null, node);
            //Console.WriteLine(node);
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
                    if (pNames == null)
                        pNames = this.Context.SqlParamNamer.Next();
                    else
                        pNames = pNames + "," + this.Context.SqlParamNamer.Next();
                }
            }
            else
            {
                pNames = this.Context.SqlParamNamer.Next();
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
