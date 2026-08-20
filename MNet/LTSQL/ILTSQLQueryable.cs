using System.Collections.Generic;
using MNet.LTSQL.SqlQueryStructs;

namespace MNet.LTSQL
{
    /// <summary>
    /// LTSQL 接口
    /// </summary>
    public interface ILTSQLQueryable
    {
        //保存查询的结构
        public QueryPart Query { get; set; }
    }

    //
    public interface ILTSQLQueryable<T> : ILTSQLQueryable
    { }

    public interface ILTSQLObjectSetable<T> : ILTSQLQueryable<T>
    {
        public QuerySetPart SetQuery { get; }
    }

    public interface ILTSQLObjectQueryable<T> : IEnumerable<T>, ILTSQLQueryable<T>
    {
        public SqlQueryPart SqlQuery { get; }
    }

    /// <summary>
    /// 排序接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILTSQLOrderedQueryable<T> : ILTSQLObjectQueryable<T>
    { }

    /// <summary>
    /// LTSQL 非查询对象(update, delete) 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILTSQLNonQueryable<T> : ILTSQLQueryable
    { }
}
