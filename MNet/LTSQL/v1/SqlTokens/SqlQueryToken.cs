using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class SqlQueryToken　: SQLValueToken
    {
        public List<SelectItemToken> DefaultFields { get; set; }

        public LTSQLToken From { get; set; }
        public LTSQLToken Where { get; set; }
        public LTSQLToken Group { get; set; }
        public LTSQLToken Order { get; set; }
        public LTSQLToken Select { get; set; }

        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return (new LTSQLToken[] { this.From, this.Where, this.Group, this.Order, this.Select }).Where(p => p != null);
        }
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
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitSqlQueryToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.From = this.From?.Visit(visitor);
            this.Where = this.Where?.Visit(visitor);
            this.Group = this.Group?.Visit(visitor);
            this.Order = this.Order?.Visit(visitor);
            this.Select = this.Select?.Visit(visitor);
            return this;
        }
    }
}
