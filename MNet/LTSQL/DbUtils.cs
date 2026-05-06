using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MNet.LTSQL
{
    public static class DbUtils
    {
        public static string Escape(string word, DbTypes dbType)
        {
            return dbType switch
            {
                DbTypes.MySQL => $"`{word}`",
                DbTypes.MSSQL => $"[{word}]",
                DbTypes.PGSQL | DbTypes.Oracle | DbTypes.SQLLite => $"\"{word}\"",
                _ => $"\"{word}\""
            };
        }

        public static string ToSqlPart(object ob, DbTypes dbType)
        {
            if (ob == null)
                return "NULL";
            else if (ob is string s)
                return $"'{s.Replace("'", "''")}'";
            else if (ob is char c)
                return $"'{c.ToString().Replace("'", "''")}'";
#if NET5_0_OR_GREATER
            else if (ob is DateOnly dl)
                return $"'{dl:yyyy-MM-dd}'";
#endif
            else if (ob is DateTime dt)
                return $"'{dt:yyyy-MM-dd HH:mm:ss}'";
            else if (ob is DateTimeOffset offset)
                return $"'{offset:yyyy-MM-dd HH:mm:ss}'";
            else if (ob is bool b)
                return b ? "1" : "0";
            else
                return ob.ToString();
        }
    }
}
