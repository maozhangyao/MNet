using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class FromToken : LTSQLToken
    {
        public Type SourceType { get; set; }
        //查询语句
        public LTSQLToken Source { get; set; }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { this.Source };
        }
        public override void ToSql(LTSQLTokenContext context)
        {
            context.SQLBuilder.Append("FROM ");
            this.Source.ToSql(context);
        }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitFromToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.Source = this.Source.Visit(visitor);
            return this;
        }
    }
}
