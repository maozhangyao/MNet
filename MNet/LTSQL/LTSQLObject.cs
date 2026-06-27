using System;
using System.Collections;
using System.Collections.Generic;
using MNet.LTSQL.SqlQueryStructs;

namespace MNet.LTSQL
{
    internal class LTSQLObject<T> : ILTSQLOrderedQueryable<T>, ILTSQLObjectSetable<T>, ILTSQLNonQueryable<T>
    {
        public LTSQLObject(SqlQueryPart query)
        {
            this.Query = query;
        }
        public LTSQLObject(QuerySetPart query)
        {
            this.Query = query;
        }
        public LTSQLObject(NonQueryPart nonQuery)
        {
            this.Query = nonQuery;
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


    public interface ILTSQLQueryable
    {
        //保存查询的结构
        public QueryPart Query { get; set; }
    }
    //保留泛型信息
    public interface ILTSQLQueryable<T>　: ILTSQLQueryable
    {　}

    public interface ILTSQLObjectSetable<T> : ILTSQLQueryable<T>
    {
        public QuerySetPart SetQuery { get; }
    }
    public interface ILTSQLObjectQueryable<T> : IEnumerable<T>, ILTSQLQueryable<T>
    {
        public SqlQueryPart SqlQuery { get; }
    }
    public interface ILTSQLOrderedQueryable<T> : ILTSQLObjectQueryable<T>
    { }


    public interface ILTSQLNonQueryable<T> : ILTSQLQueryable
    { }
}
