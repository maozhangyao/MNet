using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class PageToken : LTSQLToken
    {
        public PageToken()
        { }
        public PageToken(int? skip, int? take)
        {
            this.Skip = skip;
            this.Take = take;
        }

        public int? Skip { get; set; }
        public int? Take { get; set; }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return Array.Empty<LTSQLToken>();
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitPageToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            return this;
        }
    }
}
