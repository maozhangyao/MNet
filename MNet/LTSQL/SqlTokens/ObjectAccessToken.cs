using System;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
    // sql 对象的访问
    public class ObjectAccessToken : SqlValueToken
    {
        internal ObjectAccessToken(LTSQLToken obj, string prop, Type valueOfType) 
        {
            this.Prop = prop;
            this.Object = obj;
            this.ValueType = valueOfType;
        }

        ////表名
        public LTSQLToken Owner { get; set; }
        public string Prop{ get; set; }
        public LTSQLToken Object { get; set; }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return null;
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitObjectAccessToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.Object = this.Object.Visit(visitor);
            return this;
        }
    }
}
