using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.SqlExpression
{
    /// <summary>
    /// 构造Sql
    /// </summary>
    public interface ISqlBuilder
    {
        string Build<T>(IDbSet<T> set, SqlOptions options) where T : class;
    }

    internal class SqlBuilder : ISqlBuilder
    {
        public string Build<T>(IDbSet<T> set, SqlOptions options) where T : class
        {

            return "";
        }
    }
}
