using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.SqlTokens
{
    public class UpdateClauseToken : LTSQLToken
    {
        internal UpdateClauseToken(TableObjectToken table, TupleToken setClause, LTSQLToken whereClause)
        {
            this.Table = table;
            this.SetClause = setClause;
            this.WhereClause = whereClause;
        }

        public TableObjectToken Table { get; }
        public TupleToken SetClause { get; }
        public LTSQLToken WhereClause { get; }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitUpdateClauseToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            TableObjectToken table = this.Table?.Visit(visitor) as TableObjectToken;
            TupleToken setClause = this.SetClause?.Visit(visitor) as TupleToken;
            LTSQLToken whereCaluse = this.WhereClause?.Visit(visitor);

            return new UpdateClauseToken(table, setClause, whereCaluse);
        }
    }
}
