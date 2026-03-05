using System;
using System.Linq;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class SelectToken : LTSQLToken
    {
        public SelectToken()
        { }

        // 是否 select * 
        public bool AllFields { get; set; }
        public LTSQLToken Field { get; set; }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { this.Field};
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitSelectToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.Field = this.Field.Visit(visitor);
            return this;
        }
    }
}
