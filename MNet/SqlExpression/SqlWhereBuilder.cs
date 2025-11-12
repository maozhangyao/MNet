using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Reflection;
using System.Xml.Linq;

namespace MNet.SqlExpression
{
    /// <summary>
    /// where 表达式
    /// </summary>
    public class SqlWhereBuilder : ExpressionVisitor
    {
        private class SqlPart
        {
            public SqlPart(string part, object value = null, bool dv = false)
            {
                this.part = part;
                this.value = value;
                this.dynamicValue = dv;
            }

            //节点转成为对应的sql部分
            public string part { get; set; }
            //对节点动态求出的值，仅对捕获变量有用
            public object value { get; set; }
            //是否为动态值，仅对捕获变量有用（因为存在动态值等于null的情况，这与非动态值的情况下冲突）
            public bool dynamicValue { get; set; }
        }


        private Stack<SqlPart> _nums; //操作数(sql参数)
        private int _paramerterCount = 0; //参数指数
        private Dictionary<string, object> _sqlParameters;
        private DbType _dbType = DbType.Mysql;



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

            string pName = $"@p{_paramerterCount++}";
            this.AddParameter(pName, val);
            return pName;
        }
        private void AddParameter(string pName, object val)
        {
            if (this._sqlParameters.ContainsKey(pName))
                throw new Exception($"已经存在相同名称的参数{pName}");

            this._sqlParameters.Add(pName, val);
        }
        private string Map(MemberInfo member, SqlPart[] subPart)
        {
            var keys = FunctionMapping.GetFunctionMapKey(member);
            if (keys == null /*|| keys.Length == 0*/)
                throw new Exception($"该成员无法生成Map Key值:{member.Name}");

            string key = keys;
            if (!FunctionMapping.FunctionMaps.TryGetValue(key, out var map) || map == null)
                throw new Exception($"该成员对应的SQL函数映射处理:{member.Name}");

            string part = map(_dbType, member, subPart.Select(p => p.part).ToArray());
            return part;
        }
        protected override Expression VisitConstant(ConstantExpression node)
        {
            base.VisitConstant(node);

            //得到常量值
            if (node.NodeType == ExpressionType.Constant)
            {
                Type t = node.Value!.GetType();
                if (t == typeof(string)
                    || t.IsPrimitive)
                {
                    this._nums.Push(new SqlPart(null, node.Value, true));
                    //Console.WriteLine(node);
                }
            }

            return node;
        }
        protected override Expression VisitMember(MemberExpression node)
        {
            base.VisitMember(node);

            //访问非字面量的常量，如闭包中的捕获变量
            if (node.Expression is ConstantExpression constExpr && node.Member is FieldInfo field)
            {
                object inst = constExpr.Value;
                object value = field.GetValue(inst);
                //string pName = this.AddParameter(value);

                this._nums.Push(new SqlPart(null, value, true));
            }
            //访问参数的字段或者属性(表示为数据库表的字段)
            else if (node.Expression is ParameterExpression param)
            {
                string fieldName = DbUtils.Escape(node.Member.Name, DbType.Mysql);
                this._nums.Push(new SqlPart(fieldName));
            }
            //静态成员访问(动态求值，比如捕获变量的属性值)
            else if (node.Expression == null)
            {
                object val = this.TakeMemberValue(null, node.Member);
                this._nums.Push(new SqlPart(null, val, true));
            }
            //实例成员访问
            else
            {
                SqlPart subPart = this._nums.Pop();
                if(!subPart.dynamicValue)
                {
                    string part = this.Map(node.Member, new SqlPart[] { subPart });
                    this._nums.Push(new SqlPart(part, null, false));
                }
                else
                {
                    //动态求值，继续下一层次的动态求值
                    object val = this.TakeMemberValue(subPart.value, node.Member);
                    this._nums.Push(new SqlPart(null, val, subPart.dynamicValue));
                }
            }

            //Console.WriteLine(node);
            return node;
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            //将按照，先访问对象，在访问参数的顺序访问节点
            base.VisitMethodCall(node);

            //对于静态函数，是没有示例对象的，所以不需要额外弹出
            int args = node.Arguments.Count + (node.Object == null ? 0 : 1);
            Stack<SqlPart> stack = new Stack<SqlPart>();
            for (int i = 0; i < args; i++)
            {
                //弹出调用对象，以及参数
                SqlPart sqlPart = this._nums.Pop();
                if (sqlPart.dynamicValue)//将动态值转换为参数
                    sqlPart.part = this.AddParameter(sqlPart.value);

                stack.Push(sqlPart);
            }

            string part = this.Map(node.Method, stack.ToArray());
            this._nums.Push(new SqlPart(part));
            //Console.WriteLine(node);
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
                _ => throw new NotImplementedException($"操作符{node.NodeType}未实现")
            };

            SqlPart right = this._nums.Pop();
            SqlPart left = this._nums.Pop();
            if (right.dynamicValue)
                right.part = this.AddParameter(right.value);
            if (left.dynamicValue)
                left.part = this.AddParameter(left.value);

            this._nums.Push(new SqlPart($"({left.part} {opt} {right.part})"));
            //Console.WriteLine(node);
            return node;
        }

        public string Build(Expression expr)
        {
            this._nums = new Stack<SqlPart>();
            this._sqlParameters = new Dictionary<string, object>();

            this.Visit(expr);
            string sql = this._nums.Pop().part;

            //Console.WriteLine("************************************************************************************");
            //Console.WriteLine(sql);
            //foreach (var paras in this._sqlParameters)
            //{
            //    Console.WriteLine($"{paras.Key}: {(paras.Value == null ? null : paras.Value)}");
            //}
            //Console.WriteLine("************************************************************************************");

            return sql;
        }
    }
}
