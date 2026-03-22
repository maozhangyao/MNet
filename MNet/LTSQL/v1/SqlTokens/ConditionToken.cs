using System;
using System.Linq;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class ConditionToken : SqlValueToken
    {
        // AND , RO  = , > , < , >= , <= , <> , IN , NOT IN, LIKE , IS NULL , IS NOT NULL , BETWEEN , EXISTS
        // NOT EXISTS
        public ConditionToken() 
        {
            this.ValueType = typeof(bool);
        }
        public ConditionToken(LTSQLToken left, LTSQLToken right, string opt)
        {
            this.Left = left;
            this.Right = right;
            this.ConditionType = opt;
            this.ValueType = typeof(bool);
        }


        public readonly static string OPT_AND = "AND";
        public readonly static string OPT_OR = "OR";

        public readonly static string OPT_EQUAL = "=";
        public readonly static string OPT_NOT_EQUAL = "<>";
        public readonly static string OPT_GREATER = ">";
        public readonly static string OPT_GREATER_OR_EQUAL = ">=";
        public readonly static string OPT_LESS = "<";
        public readonly static string OPT_LESS_OR_EQUAL = "<=";
        
        public readonly static string OPT_IN = "IN";
        public readonly static string OPT_NOT_IN = "NOT IN";
        public readonly static string OPT_LIKE = "LIKE";
        public readonly static string OPT_NOT_LIKE = "NOT LIKE";
        public readonly static string OPT_IS = "IS";
        public readonly static string OPT_IS_NOT = "IS NOT";
        public readonly static string OPT_BETWEEN = "BETWEEN";
        public readonly static string OPT_NOT_BETWEEN = "NOT BETWEEN";
        public readonly static string OPT_EXISTS = "EXISTS";
        public readonly static string OPT_NOT_EXISTS = "NOT EXISTS";


        public string ConditionType { get; set; }
        //exists 运算没有 left
        public LTSQLToken Left { get; set; }
        public LTSQLToken Right { get; set; }


        public static string Not(string opt)
        {
            if (opt == OPT_AND)
                return OPT_OR;
            if (opt == OPT_OR)
                return OPT_AND;

            if (opt == OPT_EQUAL)
                return OPT_NOT_EQUAL;
            if (opt == OPT_NOT_EQUAL)
                return OPT_EQUAL;

            if (opt == OPT_GREATER)
                return OPT_LESS_OR_EQUAL;
            if (opt == OPT_GREATER_OR_EQUAL)
                return OPT_LESS;

            if (opt == OPT_LESS)
                return OPT_GREATER_OR_EQUAL;
            if (opt == OPT_LESS_OR_EQUAL)
                return OPT_GREATER;

            if (opt == OPT_IN)
                return OPT_NOT_IN;
            if (opt == OPT_NOT_IN)
                return OPT_IN;

            if (opt == OPT_LIKE)
                return OPT_NOT_LIKE;
            if (opt == OPT_NOT_LIKE)
                return OPT_NOT_LIKE;

            if (opt == OPT_IS)
                return OPT_IS_NOT;
            if (opt == OPT_IS_NOT)
                return OPT_IS;

            if (opt == OPT_BETWEEN)
                return OPT_NOT_BETWEEN;
            if (opt == OPT_NOT_BETWEEN)
                return OPT_BETWEEN;

            if (opt == OPT_EXISTS)
                return OPT_NOT_EXISTS;
            if (opt == OPT_NOT_EXISTS)
                return OPT_EXISTS;

            return opt;
        }
        public ConditionToken Not()
        {
            return new ConditionToken(this.Left, this.Right, Not(this.ConditionType));
        }
        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { this.Left, this.Right }.Where(p => p != null);
        }   
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitConditionToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.Left = this.Left?.Visit(visitor); // 比如 exists 就没有 left
            this.Right = this.Right?.Visit(visitor);
            return this;
        }
    }
}
