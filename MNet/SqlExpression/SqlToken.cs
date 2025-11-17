using System;
using System.Linq;
using System.Linq.Expressions;

namespace MNet.SqlExpression
{
    /// <summary>
    /// SQL token，对应一个表达式，该表达式可能翻译成 SQL 的一部分
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
        public SqlToken(string part, object obj, Expression expr)
        {
            this.SqlPart = part;
            this.Dynamic = obj;
            this.Expr = expr;
        }


        /// <summary>
        /// 对应的表达式树的节点
        /// </summary>
        public Expression Expr { get; set; }
        /// <summary>
        /// 已经生成的sql
        /// </summary>
        public string SqlPart { get; set; }
        /// <summary>
        /// 动态值，还不能生成sql
        /// </summary>
        public object Dynamic { get; set; }
        /// <summary>
        /// 动态值的类型(因为Dynamic可能为null,所有无法通过GetType方法来获取其类型)
        /// </summary>
        public object DynamicType { get; set; }
        /// <summary>
        /// 是否为动态值
        /// </summary>
        public bool IsDynamic => this.Dynamic != null && this.SqlPart == null;
    }
}
