using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// where 子句或者 having 子句
    /// </summary>
    public class WhereToken : LTSQLToken
    {
        public WhereToken()
        { }
        public WhereToken(string whereOrHaving, LTSQLToken condition)
        {
            this.Condition = condition;
            this.WhereOrHaving = whereOrHaving ?? "WHERE";
        }


        public string WhereOrHaving { get; set; }
        public LTSQLToken Condition { get; set; }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { this.Condition };
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitWhereToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.Condition = this.Condition.Visit(visitor);
            return this;
        }
    }
}
