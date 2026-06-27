using MNet.LTSQL;
using System;
using System.Data;

namespace DapperQ
{
    /// <summary>
    /// sql 上下文
    /// </summary>
    public class SqlContext : ISqlContext
    {
        public SqlContext() : this(null, null)
        { }
        public SqlContext(LTSQLOptions option) : this(option, null)
        { }
        public SqlContext(IDbConnection connection) : this(null, connection)
        {
        }
        public SqlContext(LTSQLOptions option, IDbConnection connection)
        {
            this.Options = option;
            this._connection = connection;
            this.Configuration();
        }


        private IDbConnection _connection;


        public LTSQLOptions? Options { get; set; }
        public Action<string> Log { get; set; }
        public virtual IDbConnection Connection => this._connection;



        protected virtual void Configuration()
        {

        }
        public virtual ILTSQLObjectQueryable<T> Create<T>() where T : class, new()
        {
            var query =  LTSQLQueryableExtensions.AsLTSQL<T>();
            query.Query.Follow = this;
            return query;
        }
        public virtual void Dispose()
        {
            this._connection?.Dispose();
        }

    }
}