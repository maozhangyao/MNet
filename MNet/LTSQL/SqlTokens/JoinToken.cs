using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
    // 联表
    public class JoinToken : LTSQLToken 
    {
        public JoinType JoinType { get; set; }
        public LTSQLToken JoinKeys { get; set; }
        public LTSQLToken MainQuery { get; set; }
        public LTSQLToken JoinQuery { get; set; }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitJoinToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.MainQuery = this.MainQuery.Visit(visitor);
            this.JoinQuery = this.JoinQuery?.Visit(visitor);
            this.JoinKeys = this.JoinKeys?.Visit(visitor);
            return this;
        }
    }

}
