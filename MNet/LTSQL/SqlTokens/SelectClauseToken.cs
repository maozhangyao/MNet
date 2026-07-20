using System;
using System.Linq;
using System.Collections.Generic;
using MNet.LTSQL.SqlTokenExtends;
using System.Security.Cryptography.X509Certificates;

namespace MNet.LTSQL.SqlTokens
{
    public class SelectClauseToken : ClauseToken
    {
        internal SelectClauseToken(LTSQLToken[] fields, LTSQLToken distinct = null, LTSQLToken topClause = null)
            : base("SELECT", new LTSQLToken[] { distinct, topClause, LTSQLTokenFactory.CreateListToken(fields) }.Where(p => p != null).ToArray())
        { }


        protected internal override ClauseToken VisitChildren(LTSQLToken[] newSubClause)
        {
            LTSQLToken[] fields = null;
            LTSQLToken distinct = null;
            LTSQLToken topClause = null;
            foreach (LTSQLToken sub in newSubClause)
            {
                if (sub is IContainerable container)
                    fields = container.ToArray();
                else if (sub is DistinctToken dist)
                    distinct = dist;
                else if (sub is TopClauseToken top)
                    topClause = top;
            }
            return new SelectClauseToken(fields, distinct, topClause);
        }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitSelectClauseToken(this);
        }
    }
}
