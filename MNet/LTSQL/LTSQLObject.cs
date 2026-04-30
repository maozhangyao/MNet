using System;
using System.Collections;
using System.Collections.Generic;
using MNet.LTSQL.SqlQueryStructs;

namespace MNet.LTSQL
{
    internal class LTSQLObject<T> : ILTSQLOrderedQueryable<T>, ILTSQLObjectSetable<T>
    {
        public LTSQLObject(SqlQueryPart query)
        {
            this.Query = query;
        }

        public LTSQLObject(QuerySetPart query)
        {
            this.Query = query;
        }


        //默认左外链接
        public JoinType JoinFlag { get; set; } = JoinType.LeftJoin;
        public QueryPart Query { get; set; }
        public SqlQueryPart SqlQuery => Query as SqlQueryPart;
        public QuerySetPart SetQuery => Query as QuerySetPart;

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public interface ILTSQLObjectQueryable
    {
        //保存查询的结构
        public QueryPart Query { get; set; }
    }
    public interface ILTSQLObjectSetable<T> : ILTSQLObjectQueryable
    {
        public QuerySetPart SetQuery { get; }
    }
    public interface ILTSQLObjectQueryable<T> : IEnumerable<T>, ILTSQLObjectQueryable
    {
        public SqlQueryPart SqlQuery { get; }
    }
    public interface ILTSQLOrderedQueryable<T> : ILTSQLObjectQueryable<T>
    { }
}
