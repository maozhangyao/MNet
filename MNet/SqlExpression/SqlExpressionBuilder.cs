using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Reflection;
using System.Xml.Linq;

namespace MNet.SqlExpression
{
    /// <summary>
    /// where 表达式
    /// </summary>
    public class SqlExpressionBuilder : CommonSqlExpressionBuilder
    {
        public SqlExpressionBuilder(SqlBuildContext context)
        {
            this.Context = context;
        }

        private void Clear()
        {
            this.Stack.Clear();
            this.Paramters.Clear();
        }


        private SqlToken Begin(Expression expr)
        {
            this.Visit(expr);
            return this.PopToken();
        }

        //生成where
        public string BuildWhere(Expression expr)
        {
            return this.Begin(expr).SqlPart;
        }
        //生成 order by
        public string BuildOrder(IEnumerable<DbSetOrder> orders)
        {
            List<string> orderbys = new List<string>();
            foreach (DbSetOrder order in orders)
            {
                this.Clear();
                
                SqlToken token = this.Begin(order.OrderByExpress);

                orderbys.Add($"{token.SqlPart}{(order.IsDesc ? " desc" : "")}");
            }
            return string.Join(",", orderbys);
        }
        public string BuildFrom()
        {
            return "*";
        }
        public string BuildSelect()
        {
            return "*";
        }
    }
}
