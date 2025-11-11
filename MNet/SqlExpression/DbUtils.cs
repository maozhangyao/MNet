using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MNet.SqlExpression
{
    public static class DbUtils
    {
        public static string Escape(string word, DbType dbType)
        {
            return dbType switch
            {
                DbType.Mysql => $"`{word}`",
                DbType.Mssql => $"[{word}]",
                _ => throw new NotImplementedException($"未实现{dbType}数据库类型的关键字转义"),
            };
        }
    }
}
