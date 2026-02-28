using System;
using System.Linq;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class SelectToken : LTSQLToken
    {
        public SelectToken()
        { }

        // 是否 select * 
        public bool AllFields { get; set; }
        public LTSQLToken Field { get; set; }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { this.Field};
        }
        public override void ToSql(LTSQLTokenContext context)
        {
            context.SQLBuilder.Append("SELECT ");

            if (this.AllFields)
            {
                context.SQLBuilder.Append("*");
            }
            else
            {
                this.Field.ToSql(context);
            }
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitSelectToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.Field = this.Field.Visit(visitor);
            return this;
        }
    }


    public class SelectItemToken : LTSQLToken
    {
        public SelectItemToken() 
        { }
        public SelectItemToken(LTSQLToken field, string fieldAlias)
        {
            Field = field;
            FieldAlias = fieldAlias;
        }

        public LTSQLToken Field { get; set; }
        public string FieldAlias { get; set; }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { this.Field };
        }
        public override void ToSql(LTSQLTokenContext context)
        {
            this.Field.ToSql(context);
            context.SQLBuilder.Append(' ');
            context.SQLBuilder.Append(this.FieldAlias);
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitSelectItemToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.Field = this.Field.Visit(visitor);
            return this;
        }
    }
}
