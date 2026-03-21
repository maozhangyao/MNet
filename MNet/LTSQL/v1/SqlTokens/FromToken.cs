using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class FromToken : LTSQLToken
    {
        public Type FromType { get; set; }
        public LTSQLToken Source { get; set; }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { this.Source };
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitFromToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.Source = this.Source?.Visit(visitor);
            return this;
        }
    }
}
