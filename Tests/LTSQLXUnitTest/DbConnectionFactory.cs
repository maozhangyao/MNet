using DapperQ;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace LTSQLXUnitTest
{
    internal class DbConnectionFactory
    {
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
            return new SqlContext(new MNet.LTSQL.LTSQLOptions()
            {
                UseSqlParameter = false,
                DbType = MNet.LTSQL.DbTypes.SQLLite,
            }, Sqllite())
            { 
                Log = logs
            };
        }
    }
}
