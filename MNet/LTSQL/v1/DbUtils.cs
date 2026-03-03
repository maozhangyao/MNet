using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MNet.LTSQL.v1
{
    public static class DbUtils
    {
        public static string Escape(string word, DbType dbType)
        {
            return dbType switch
            {
                DbType.MySQL => $"`{word}`",
                DbType.MSSQL => $"[{word}]",
                _ => throw new NotImplementedException($"未实现{dbType}数据库类型的关键字转义"),
            };
        }

        public static string ToSqlPart(object ob, DbType dbType)
        {
            if (ob == null)
                return "NULL";
            else if (ob is string s)
                return $"'{s.Replace("'", "''")}'";
            else if (ob is DateTime dt)
                return $"'{dt:yyyy-MM-dd HH:mm:ss}'";
            else if(ob is DateTimeOffset offset)
                return $"'{offset:yyyy-MM-dd HH:mm:ss}'";
            else if (ob is bool b)
                return b ? "1" : "0";
            else
                return ob.ToString();
        }
    }
}
