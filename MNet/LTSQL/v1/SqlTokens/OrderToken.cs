using System;
using System.Linq;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class OrderToken : LTSQLToken
    {
        public LTSQLToken[] OrderByItems { get; set; }
        public LTSQLToken OrderBy { get; set; }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { this.OrderBy };
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitOrderToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.OrderBy.Visit(visitor);
            return this;
        }
    }
}
