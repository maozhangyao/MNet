using MNet.Kits;
using MNet.LTSQL;
using System;
using System.Data;
using System.Data.Common;

namespace DapperQ
{
    /// <summary>
    /// sql 上下文
    /// </summary>
    public class SqlContext : ISqlContext
    {
        protected SqlContext() : this(new SqlContextOptions()) 
        { }
        public SqlContext(IObjectRenting<IDbConnection> dbConnectionRenting) : this(new SqlContextOptions() { DbConnectionRenting = dbConnectionRenting })
        { }
        public SqlContext(LTSQLOptions option, IObjectRenting<IDbConnection> dbConnectionRenting) : this(new SqlContextOptions() { LTSQLOptions = option, DbConnectionRenting = dbConnectionRenting })
        { }
        public SqlContext(SqlContextOptions options)
        {
            this.Configuration(options) ;
            if (options.DbConnectionRenting == null)
                throw new Exception($"实例化{nameof(SqlContext)}类时，请配置{nameof(IObjectRenting<IDbConnection>)}连接器。");

            this._disposed = false;
            this.Options = options.LTSQLOptions;
            this._db = options.DbConnectionRenting;
            this.Log = options.Log ?? ((string msg) => { });
        }


        private bool _disposed;
        private IObjectRenting<IDbConnection> _db;


        public Action<string> Log { get; set; }
        public LTSQLOptions? Options { get; set; }
        public virtual IObjectRenting<IDbConnection> ConnectionRenting
        {
            get
            {

                this.ThrowIfDisposed();
                return this._db;
            }
        }


        private void ThrowIfDisposed()
        {
            if (this._disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }
        protected virtual void Configuration(SqlContextOptions optins)
        {
            
        }
        public virtual ILTSQLNonQueryable<T> CreateUpdate<T>()
        {
            this.ThrowIfDisposed();

            var update = LTSQLQueryableExtensions.AsUpdate<T>(p => p);
            update.Query.Follow = this;
            return update;
        }
        public virtual ILTSQLNonQueryable<T> CreateDelete<T>()
        {
            this.ThrowIfDisposed();

            var delete = LTSQLQueryableExtensions.AsDelete<T>();
            delete.Query.Follow = this;
            return delete;
        }
        public virtual ILTSQLObjectQueryable<T> CreateQuery<T>()
        {
            this.ThrowIfDisposed();

            var query =  LTSQLQueryableExtensions.AsLTSQL<T>();
            query.Query.Follow = this;
            return query;
        }
        public virtual ILTSQLQueryable Follow(ILTSQLQueryable expr)
        {
            this.ThrowIfDisposed();

            expr.Query.Follow = this;
            return expr;
        }
        public virtual void Dispose()
        {
            if (this._disposed)
                return;

            this._disposed = true;
            this._db?.Dispose();
        }

        ~SqlContext()
        {
            this.Dispose();
        }
    }
}