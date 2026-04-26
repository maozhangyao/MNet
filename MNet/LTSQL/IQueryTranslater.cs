using System;
using MNet.LTSQL.SqlQueryStructs;
using MNet.LTSQL.SqlTokens;

namespace MNet.LTSQL
{
    /// <summary>
    /// 翻译查询对象
    /// </summary>
    public interface IQueryTranslater
    {
        LTSQLToken Translate(QueryPart query, LTSQLOptions options);
        LTSQLToken Translate(QueryPart query, LTSQLTranslateScope scope);
    }
}

