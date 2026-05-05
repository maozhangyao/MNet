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
    }
}
