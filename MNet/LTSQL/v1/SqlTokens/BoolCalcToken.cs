using System;
using System.Linq;
using System.Collections.Generic;
using MNet.LTSQL.v1.SqlTokenExtends;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class BoolCalcToken : BinaryToken, INotable
    {
        // AND , RO  = , > , < , >= , <= , <> , IN , NOT IN, LIKE , IS NULL , IS NOT NULL , BETWEEN , EXISTS
        // NOT EXISTS
        public BoolCalcToken() 
        {
            this.ValueType = typeof(bool);
        }
        public BoolCalcToken(LTSQLToken left, LTSQLToken right, string opt)
        {
            this.Left = left;
            this.Right = right;
            this.ConditionType = opt;
            this.Opration = opt;
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


        public LTSQLToken Not()
        {
            return new BoolCalcToken(this.Left, this.Right, Not(this.ConditionType));
        }
        public static string Not(string opt)
        {
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

            throw new Exception($"操作符：{opt}不支持Not取反操作。");
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitBoolCalcToken(this);
        }
    }
}
