using MNet.LTSQL.SqlTokenExtends;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MNet.LTSQL.SqlTokens
{
    // sql 对象的访问
    public class AccessPropertyToken : SqlValueToken
    {
        internal AccessPropertyToken(LTSQLToken obj, FieldToken prop)
        {
            this.Object = obj;
            this.Prop = prop;
            this.ValueType = prop.FieldValueType;
        }

        public FieldToken Prop { get; }
        public LTSQLToken Object { get; }


        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitAccessPropertyToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            var newObject = this.Object.Visit(visitor);
            var newField = this.Prop.Visit(visitor) as FieldToken;
            if (newField == null)
                throw new Exception($"{nameof(AccessPropertyToken)}: {newField.GetType().FullName}类型无法转换为{typeof(FieldToken).FullName}");

            return new AccessPropertyToken(newObject, newField) { IsPriority = this.IsPriority };
        }
        protected override string ToString(string fmt)
        {
            string c = this.Object.ToString() + "." + this.Prop.FieldName;
            return string.Format(fmt, c);
        }
    }
}
