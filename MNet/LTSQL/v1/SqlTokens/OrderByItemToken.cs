using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class OrderByItemToken : LTSQLToken
    {
        public LTSQLToken Item { get; set; }
        public bool IsAsc { get; set; } = true;


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { this.Item };
        }
        public override void ToSql(LTSQLTokenContext context)
        {
            this.Item.ToSql(context);
            context.SQLBuilder.Append(' ');
            if (!this.IsAsc)
                context.SQLBuilder.Append("desc");
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitOrderByItemToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.Item = this.Item.Visit(visitor);
            return this;
        }
    }
}
