using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using MNet.LTSQL.SqlTokenExtends;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 二元操作
    /// </summary>
    public class BinaryToken : SqlValueToken, IPriorable
    {
        internal BinaryToken(string opt, LTSQLToken left, LTSQLToken right, Type typeOfValue)
            : this(opt, left, right, typeOfValue, false)
        { }
        internal BinaryToken(string opt, LTSQLToken left, LTSQLToken right, Type typeOfValue, bool priority)
        {
            this.Opration = opt;
            this.Left = left;
            this.Right = right;
            this.ValueType = typeOfValue;
            this.IsPriority = priority;
        }


        public readonly string Opration;
        //exists 运算没有 left
        public readonly LTSQLToken Left;
        public readonly LTSQLToken Right;

        //标准的二元操作符
        public readonly static string OPT_ADD = "+";
        public readonly static string OPT_SUBTRACT = "-";
        public readonly static string OPT_DIVIDE = "/";
        public readonly static string OPT_MULTIPLY = "*";
        public readonly static string OPT_EQUAL = "=";
        public readonly static string OPT_NOT_EQUAL = "<>";
        public readonly static string OPT_GREATER = ">";
        public readonly static string OPT_GREATER_OR_EQUAL = ">=";
        public readonly static string OPT_LESS = "<";
        public readonly static string OPT_LESS_OR_EQUAL = "<=";
        public readonly static string OPT_AND = "AND";
        public readonly static string OPT_OR = "OR";
        public readonly static string OPT_IN = "IN";
        public readonly static string OPT_NOT_IN = "NOT IN";
        public readonly static string OPT_LIKE = "LIKE";
        public readonly static string OPT_NOT_LIKE = "NOT LIKE";
        public readonly static string OPT_IS = "IS";
        public readonly static string OPT_IS_NOT = "IS NOT";
        public readonly static string OPT_BETWEEN = "BETWEEN";
        public readonly static string OPT_NOT_BETWEEN = "NOT BETWEEN";


        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitBinaryToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            LTSQLToken left = this.Left?.Visit(visitor);
            LTSQLToken right = this.Right?.Visit(visitor);
            return this.VisitChildren(left, right);
        }
        protected internal virtual BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new BinaryToken(this.Opration, newLeft, newRight, this.ValueType, this.IsPriority);
        }
       

        protected override string ToString(string fmt)
        {
            string val = $"{this.Left} {this.Opration} {this.Right}";
            return string.Format(fmt, val);
        }
    }
}
