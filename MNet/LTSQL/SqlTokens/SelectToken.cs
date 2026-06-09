using System;
using System.Linq;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
    public class SelectToken : LTSQLToken
    {
        internal SelectToken()
        { }

        public LTSQLToken Fields { get; set; }


        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitSelectToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.Fields = this.Fields.Visit(visitor);
            return this;
        }
        public override string ToString()
        {
            return this.Fields.ToString();
        }
    }
}
