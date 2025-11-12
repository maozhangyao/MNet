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
        string Build<T>(IDbSet<T> dbset, SqlOptions options) where T : class;
    }

    public class SqlBuilder : ISqlBuilder
    {

        private string BuildFrom(DbSetStrcut set, SqlOptions options)
        {
            if (set.From != null)
                return this.Build(set.From, options);

            return new RootFromBuilder().Build(set.SelectExprs); 
        }
        private string Build(DbSetStrcut set, SqlOptions options)
        {
            if (set.IsEmpty)
                return this.Build(set.From, options);

            string form = this.BuildFrom(set, options);
            if (set.IsRoot)
                return form;

            SqlWhereBuilder whereBuilder = new SqlWhereBuilder();
            string where = null;
            if (set.WhereExpr != null)
                where = whereBuilder.Build(set.WhereExpr);

            //build from
            //build where
            //build order

            SqlOrderBuilder orderBuilder = new SqlOrderBuilder();
            string order = null;
            if (set.OrderExprs?.Count > 0)
                order = orderBuilder.Build(set.OrderExprs);

            //build select
            SqlSelectBuilder selectBuilder = new SqlSelectBuilder();
            string select = null;
            if(set.SelectExprs != null)
                select = selectBuilder.Build(set.SelectExprs);

            return $@"select {select}
from {form}
where {where}
order by {order}
";

        }


        public string Build<T>(IDbSet<T> dbset, SqlOptions options) where T : class 
        {
            DbPipe<T> pipe = dbset as DbPipe<T>;
            if (pipe == null)
                throw new Exception($"{nameof(dbset)}参数不能未null");

            DbSetStrcut set = pipe.DbSet;
            return this.Build(set, options);
        }
    }
}
