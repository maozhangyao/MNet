using System;
using MNet.LTSQL.SqlTokenExtends;
using MNet.LTSQL.SqlTokens;

namespace MNet.LTSQL
{
    public static class LTSQLTokenExtensions
    {
        public static LTSQLToken TryPriority(this LTSQLToken token, bool prior)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            if (token is IPriorable priorable && priorable.IsPriority != prior)
                return priorable.SetPriority(prior) as LTSQLToken;
            return token;
        }
    }
}

