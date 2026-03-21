using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    // 联表
    public class JoinToken : LTSQLToken 
    {
        public string JoinType { get; set; }
        public LTSQLToken JoinKeys { get; set; }
        public LTSQLToken MainQuery { get; set; }
        public LTSQLToken JoinQuery { get; set; }

        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { JoinKeys, MainQuery, JoinQuery };
        }
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
