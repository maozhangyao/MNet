using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class OrderToken : LTSQLToken
    {
        public List<OrderByItemToken> OrderByItems { get; set; }
        public override void ToSql(LTSQLTokenContext context)
        {
            context.SQLBuilder.Append("ORDER BY ");

            bool comma = false;
            foreach (var order in OrderByItems)
            {
                if (comma)
                    context.SQLBuilder.Append(", ");

                comma = true;
                order.ToSql(context);
            }
            //return "Order";
        }
    }
}
