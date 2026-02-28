using System;
using System.Linq;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class OrderToken : LTSQLToken
    {
        public LTSQLToken[] OrderByItems { get; set; }



        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return this.OrderByItems?.ToArray();
        }

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
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitOrderToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            if(this.OrderByItems != null)
            {
                for (int i = 0; i < this.OrderByItems.Length; i++)
                {
                    LTSQLToken item = this.OrderByItems[i];
                    this.OrderByItems[i] = item.Visit(visitor);
                }
            }
            return this;
        }
    }
}
