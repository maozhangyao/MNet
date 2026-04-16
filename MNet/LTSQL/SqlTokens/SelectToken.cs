using System;
using System.Linq;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
    public class SelectToken : LTSQLToken
    {
        public SelectToken()
        { }


        // 是否 select * 
        public bool Asterisk { get; set; }
        //distinct 子句
        public bool Distinct { get; set; }
        //top 子句（sql server 专属）
        public int? TopLimit { get; set; }
        public LTSQLToken Fields { get; set; }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { this.Fields};
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitSelectToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.Fields = this.Fields.Visit(visitor);
            return this;
        }
    }
}
