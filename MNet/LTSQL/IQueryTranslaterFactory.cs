using System;
using MNet.LTSQL.SqlQueryStructs;

namespace MNet.LTSQL
{
    /// <summary>
    /// IQueryTranslater 构造工厂
    /// </summary>
    public interface IQueryTranslaterFactory
    {
        IQueryTranslater Create(QueryPart query);
    }
}

