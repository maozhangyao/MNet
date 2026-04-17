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

        public string Prop { get; }
        public LTSQLToken Object { get; }


        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitObjectAccessToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            var newObject = this.Object.Visit(visitor);
            if (newObject == this.Object)
                return this;
            
            return new ObjectAccessToken(newObject, this.Prop, this.ValueType);
        }
    }
}
