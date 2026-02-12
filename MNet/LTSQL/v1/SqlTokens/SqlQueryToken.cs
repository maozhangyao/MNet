using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class SqlQueryToken　: LTSQLToken
    {
        public List<SelectItemToken> DefaultFields { get; set; }

        public FromToken From { get; set; }
        public WhereToken Where { get; set; }
        public GroupToken Group { get; set; }
        public OrderToken Order { get; set; }
        public SelectToken Select { get; set; }

        public override void ToSql(LTSQLTokenContext context)
        {
            this.Select.ToSql(context);
            context.SQLBuilder.AppendLine();
            this.From.ToSql(context);

            if (this.Where != null)
            {
                context.SQLBuilder.AppendLine();
                this.Where.ToSql(context);
            }
            if(this.Group != null)
            {
                context.SQLBuilder.AppendLine();
                this.Group.ToSql(context);
            }
            if(this.Order != null)
            {
                context.SQLBuilder.AppendLine();
                this.Order.ToSql(context);
            }
        }
    }
}
