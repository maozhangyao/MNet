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
        string Build(object dbset, SqlOptions options);
        
    }

    /// <summary>
    /// 构造sql
    /// </summary>
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

            SqlBuildContext context = new SqlBuildContext();
            context.Options = options;

            SqlExpressionBuilder exprBuilder = new SqlExpressionBuilder(context);

            //where
            string where = null;
            if (set.WhereExpr != null)
                where = exprBuilder.BuildWhere(set.WhereExpr);

            //order
            string order = null;
            if (set.OrderExprs?.Count > 0)
                order = exprBuilder.BuildOrder(set.OrderExprs);

            //select
            string select = null;
            if (set.SelectExprs != null)
                select = exprBuilder.BuildSelect();

            return $@"
select {select}
where {where}
order by {order}
";
        }


        public string Build(object dbset, SqlOptions options)
        {
            DbSetStrcut set = DbSetExtensions.GetDbSetStruct(dbset);
            return this.Build(set, options);
        }
    }
}
