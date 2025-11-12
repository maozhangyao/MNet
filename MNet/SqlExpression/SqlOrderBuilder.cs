using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MNet.SqlExpression
{
    /// <summary>
    /// sql token
    /// </summary>
    public class SqlToken
    {
        public SqlToken()
        { }
        public SqlToken(string part, object obj)
        {
            this.SqlPart = part;
            this.Dynamic = obj;
        }

        /// <summary>
        /// 已经生成的sql
        /// </summary>
        public string SqlPart { get; set; }
        /// <summary>
        /// 动态值，还不能生成sql
        /// </summary>
        public object Dynamic { get; set; }

        public bool IsDynamic => this.Dynamic != null && this.SqlPart == null;
    }


    /// <summary>
    /// order by 部分生成
    /// </summary>
    public class SqlOrderBuilder : ExpressionVisitor
    {

        private Stack<SqlToken> _tokens;
        private DbType _dbType = DbType.Mysql;
        private Dictionary<string, object> _sqlParamters;
        private int _sqlCount;

        private string AddParameter(object val)
        {
            if (val is IEnumerable enumer && !(val is string))
            {
                //列表
                string pNames = null;
                foreach (var item in enumer)
                {
                    if (pNames == null)
                        pNames = this.AddParameter(item);
                    else
                        pNames = pNames + "," + this.AddParameter(item);
                }
                return pNames;
            }
            //如果值是一个复杂的类对象，则无法转换成sql参数，需要异常处理

            string pName = $"@p{_sqlCount++}";
            this.AddParameter(pName, val);
            return pName;
        }
        private void AddParameter(string pName, object val)
        {
            if (this._sqlParamters.ContainsKey(pName))
                throw new Exception($"已经存在相同名称的参数{pName}");

            this._sqlParamters.Add(pName, val);
        }


        private object TakeMemberValue(object inst, MemberInfo member)
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


        protected override Expression VisitConstant(ConstantExpression node)
        {
            base.VisitConstant(node);

            //访问到常量，该常量值需要动态求值
            this._tokens.Push(new SqlToken(null, node.Value));
            return node;
        }
        protected override Expression VisitMember(MemberExpression node)
        {
            base.VisitMember(node);

            //访问参数
            if (node.Expression is ParameterExpression parameter)
            {
                this._tokens.Push(new SqlToken(DbUtils.Escape(node.Member.Name, this._dbType), null));
            }
            //访问到了静态成员的属性或者字段
            else if (node.Expression == null)
            {
                object val = this.TakeMemberValue(null, node.Member);
                this._tokens.Push(new SqlToken(null, val));
            }
            //访问的实例成员的属性或者字段
            else
            {
                SqlToken pre = this._tokens.Pop();
                if (pre.IsDynamic)
                {
                    //对常量分支做求值
                    object val = this.TakeMemberValue(pre.Dynamic, node.Member);
                    this._tokens.Push(new SqlToken(null, val));
                }
                else
                {
                    //对参数分支做转换
                    string key = FunctionMapping.GetFunctionMapKey(node.Member);
                    string par = FunctionMapping.FunctionMaps[key](this._dbType, node.Member, new string[] { pre.SqlPart });
                    this._tokens.Push(new SqlToken(par, null));
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
                    sqlTokens.Push(this._tokens.Pop());
            }
            //调用了实例函数
            else
            {
                for (int i = 0; i <= args; i++)
                    sqlTokens.Push(this._tokens.Pop());

            }

            SqlToken[] tokens = sqlTokens.ToArray();
            //如果所有的节点都需要动态求值，则
            if (sqlTokens.All(p => p.IsDynamic) //表示所有的参数节点都是需要动态求值的，所以直接动态求值 
                || (sqlTokens.Count <= 0 && node.Object == null)) //表示调用了无参数的静态函数，直接求值即可
            {
                //这个方法是应该动态求值，还是转化为sql

                object obj = node.Object == null ? node.Method.Invoke(null, tokens.Select(p => p.Dynamic).ToArray()) //调用静态函数
                    : node.Method.Invoke(tokens[0], tokens.Skip(1).Select(p => p.Dynamic).ToArray()); //调用实例函数

                this._tokens.Push(new SqlToken(null, obj));
            }
            else
            {
                //直接转换为sql
                string key = FunctionMapping.GetFunctionMapKey(node.Method);
                string par = FunctionMapping.FunctionMaps[key](this._dbType, node.Method, sqlTokens.Select(p => p.SqlPart).ToArray());
                this._tokens.Push(new SqlToken(par, null));
            }

            return node;
        }



        public string Build(IEnumerable<DbSetOrder> orders)
        {
            this._sqlParamters = new Dictionary<string, object>();
            this._sqlCount = 0;

            List<string> orderbys = new List<string>();
            foreach(DbSetOrder order in orders)
            {
                this._tokens = new Stack<SqlToken>();
                this.Visit(order.OrderByExpress);

                SqlToken token = this._tokens.Pop();
                if (token.IsDynamic)
                    token.SqlPart = this.AddParameter(token.Dynamic);

                string orderby = token.SqlPart;
                if (order.IsDesc)
                    orderby += " desc";
                orderbys.Add(orderby);
            }

            


            return string.Join(", ", orderbys);
        }
    }
}
