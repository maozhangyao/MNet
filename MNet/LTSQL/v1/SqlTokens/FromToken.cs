using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class FromToken : LTSQLToken
    {
        public Type FromType { get; set; }
        public string JoinType { get; set; }
        public LTSQLToken JoinFrom { get; set; }
        public LTSQLToken JoinKeys { get; set; }
        public LTSQLToken Sequence { get; set; }
        
        public override IEnumerable<LTSQLToken> GetChildren()
        {
            if (this.JoinFrom != null)
                return new[] { this.JoinFrom, this.Sequence, this.JoinKeys };
            return new[] { this.Sequence };
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitFromToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.JoinFrom = this.JoinFrom?.Visit(visitor);
            this.Sequence = this.Sequence.Visit(visitor);
            this.JoinKeys = this.JoinKeys?.Visit(visitor);
            return this;
        }
    }
}
