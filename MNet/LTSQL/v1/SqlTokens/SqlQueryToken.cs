using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class SqlQueryToken　: SqlValueToken
    {
        public List<FieldInfoToken> DefaultFields { get; set; }


        public LTSQLToken From { get; set; }
        public LTSQLToken Where { get; set; }
        public LTSQLToken Group { get; set; }
        public LTSQLToken Having { get; set; }
        public LTSQLToken Order { get; set; }
        public LTSQLToken Select { get; set; }
        //分页子句
        public LTSQLToken Page { get; set; }



        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return (new LTSQLToken[] { this.From, this.Where, this.Group, this.Having, this.Order, this.Page, this.Select }).Where(p => p != null);
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
            this.Having = this.Having?.Visit(visitor);
            this.Order = this.Order?.Visit(visitor);
            this.Page = this.Page?.Visit(visitor);
            this.Select = this.Select?.Visit(visitor);
            return this;
        }
    }
}
