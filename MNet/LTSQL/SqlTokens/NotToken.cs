using System;

namespace MNet.LTSQL.SqlTokens
{
    public class NotToken : SqlValueToken 
    {
        internal NotToken(SqlValueToken valueOfBool)
        {
            if (valueOfBool == null)
                throw new ArgumentNullException(nameof(valueOfBool));

            this.Value = valueOfBool;
            this.ValueType = typeof(bool);
        }

        public readonly SqlValueToken Value;

        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            var newValue = this.Value?.Visit(visitor);
            if (newValue != null && !object.ReferenceEquals(newValue, this.Value))
            {
                return new NotToken((SqlValueToken)newValue);
            }
            return this;
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitNotToken(this);
        }
    }
}
