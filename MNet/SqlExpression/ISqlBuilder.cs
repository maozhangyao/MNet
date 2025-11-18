using System;
using System.Text;
using System.Collections.Generic;

namespace MNet.SqlExpression
{
    /// <summary>
    /// 构造Sql
    /// </summary>
    public interface ISqlBuilder
    {
        string Build(object dbset, SqlOptions options);
    }

    /// <summary>
    /// 构造sql
    /// </summary>
    internal class SqlBuilder : ISqlBuilder
    {
        private string Build(DbSetStrcut set, SqlBuildContext context)
        {
            SqlExpressionBuilder builder = new SqlExpressionBuilder(context);
            SqlDescriptor descriptor = builder.Build(set);
            return descriptor.ToSql();
        }

        public string Build(object dbset, SqlOptions options)
        {
            DbSetStrcut set = DbSetExtensions.GetDbSetStruct(dbset);
            SqlBuildContext context = new SqlBuildContext();
            context.Options = options;

            return this.Build(set, context);
        }
        public string Build(object dbset, SqlBuildContext context)
        {
            DbSetStrcut set = DbSetExtensions.GetDbSetStruct(dbset);
            return this.Build(dbset, context);
        }
    }
}
