using System;
using MNet.LTSQL.SqlTokens;

namespace MNet.LTSQL
{
    public static class LTSQLTokenExtensions
    {
        //为子查询加括号
        public static LTSQLToken PriorityIfSubQuery(this LTSQLToken token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            return token is SqlQueryToken ? LTSQLTokenFactory.CreatePriorityCalcToken(token) :
                   token is SqlParameterToken p && p.Value is ILTSQLObjectQueryable ? LTSQLTokenFactory.CreatePriorityCalcToken(token) : token;
        }
        //取消子查询的括号
        public static LTSQLToken UnPriorityIfSubQuery(this LTSQLToken token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            if (token is PriorityCalcToken p)
            {
                if (p.Value is SqlQueryToken)
                    return p.Value;
                if (p.Value is SqlParameterToken param && param.Value is ILTSQLObjectQueryable)
                    return p.Value;
            }
            return token;
        }
    }
}

