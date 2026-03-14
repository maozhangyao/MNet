using System;
using System.Collections;
using System.Collections.Generic;
using MNet.LTSQL.v1.SqlQueryStructs;

namespace MNet.LTSQL.v1
{
    internal class LTSQLObject<T> : ILTSQLOrderedQueryable<T>
    {
        public LTSQLObject() 
        { }
        public LTSQLObject(SqlQueryPart query)
        {
            this.Query = query;
        }


        public SqlQueryPart Query { get; set; }

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
        public SqlQueryPart Query { get; set; }
    }
    public interface ILTSQLObjectQueryable<T> : IEnumerable<T>, ILTSQLObjectQueryable
    { }
    public interface ILTSQLOrderedQueryable<T> : ILTSQLObjectQueryable<T>
    { }
}
