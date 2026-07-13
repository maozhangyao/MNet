using DapperQ;
using MNet.Kits;
using MNet.LTSQL;
using System.Data;
using Microsoft.Data.Sqlite;
using System;
using System.Text;
using System.Collections.Generic;

namespace LTSQLXUnitTest
{
    internal class DbConnectionFactory
    {
        private static readonly GeneralObjectPool _pool = new GeneralObjectPool(() => Sqllite(), 100);

        public static IDbConnection Sqllite()
        {
            return Sqllite("Data Source=MNetSqllite.db");
        }
        public static IDbConnection Sqllite(string conneStr)
        {
            return new SqliteConnection(conneStr);
        }

        public static ISqlContext CreateSqlContext(Action<string> logs = null)
        {
            SqlContextOptions opts = new SqlContextOptions();
            opts.LTSQLOptions = new LTSQLOptions();
            opts.LTSQLOptions.UseSqlParameter = false;
            opts.LTSQLOptions.DbType = DbTypes.SQLLite;
            opts.DbConnectionRenting = _pool.Rent<IDbConnection>();
            opts.Log = logs;

            return new SqlContext(opts);
        }
    }
}
