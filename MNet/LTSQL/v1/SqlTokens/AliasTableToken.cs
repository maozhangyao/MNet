using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class AliasTableToken : LTSQLToken
    {
        public AliasTableToken() 
        { }
        public AliasTableToken(string alias, LTSQLToken sqlObj)
        {
            this.Alias = alias;
            this.Query = sqlObj;
        }

        public LTSQLToken Query { get; set; }
        public string Alias { get; set; }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { this.Query };
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitAliasTableToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.Query = this.Query.Visit(visitor);
            return this;
        }
    }
}
