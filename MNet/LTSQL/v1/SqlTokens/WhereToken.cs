using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class WhereToken : LTSQLToken
    {
        public WhereToken()
        { }
        public WhereToken(LTSQLToken condition)
        {
            this.Condition = condition;
        }

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
