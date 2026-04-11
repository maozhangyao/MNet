using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 分页语句
    /// </summary>
    public class PageToken : LTSQLToken
    {
        internal PageToken(int? skip, int? take)
        {
            this.Skip = skip;
            this.Take = take;
        }


        public readonly int? Skip;
        public readonly int? Take;


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
