using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    // sql 对象的访问
    public class ObjectAccessToken : SqlValueToken
    {
        public ObjectAccessToken(LTSQLToken access, Type valueOfType) 
        {
            this.Access = access;
            this.ValueType = valueOfType;
        }

        ////表名
        //public LTSQLToken Owner { get; set; }
        //public LTSQLToken Field { get; set; }
        public LTSQLToken Access { get; set; }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { this.Access };
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitObjectAccessToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.Access = this.Access.Visit(visitor);
            return this;
        }
    }
}
