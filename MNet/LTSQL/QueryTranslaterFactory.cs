using MNet.LTSQL.SqlQueryStructs;

namespace MNet.LTSQL
{
    public class QueryTranslaterFactory : IQueryTranslaterFactory
    {
        /// <summary>
        /// 创建一个查询转换器
        /// </summary>
        /// <param name="query"></param>
        /// <returns>返回值可空</returns>
        public IQueryTranslater? Create(QueryPart query)
        {
            if(query is SqlQueryPart)
                return new SequenceTranslater();

            return null;
        }
    }
}

