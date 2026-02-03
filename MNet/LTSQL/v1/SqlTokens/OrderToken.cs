using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class OrderToken : LTSQLToken
    {
        public List<OrderByItemToken> OrderByItems { get; set; }
        public override string ToSql()
        {
            return "Order";
        }
    }
}
