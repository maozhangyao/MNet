using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.SqlTokens
{
    // 联表
    public class JoinToken : LTSQLToken 
    {
        internal JoinToken(JoinType joinType, LTSQLToken mainQuery, LTSQLToken joinQuery, LTSQLToken joinKeys)
        {
            this.JoinType = joinType;
            this.MainQuery = mainQuery;
            this.JoinQuery = joinQuery;
            this.JoinKeys = joinKeys;
        }


        public JoinType JoinType { get; }
        public LTSQLToken JoinKeys { get; }
        public LTSQLToken MainQuery { get; }
        public LTSQLToken JoinQuery { get; }



        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitJoinToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            var newMainQuery = this.MainQuery?.Visit(visitor);
            var newJoinQuery = this.JoinQuery?.Visit(visitor);
            var newJoinKeys = this.JoinKeys?.Visit(visitor);

            return new JoinToken(this.JoinType, newMainQuery, newJoinQuery, newJoinKeys);
        }
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine(this.MainQuery.ToString());
            b.AppendLine(this.JoinKeys.ToString());
            b.Append(this.JoinQuery.ToString());
            b.Append(" ON ");
            b.AppendLine(this.JoinQuery.ToString());
            return b.ToString();
        }
    }

}
