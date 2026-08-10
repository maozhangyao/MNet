using System;

namespace MNet.LTSQL.SqlTokens
{
    public class FieldToken : LTSQLToken
    {
        internal FieldToken(string fieldName, Type fieldValueType)
        {
            this.FieldName = fieldName;
            this.FieldValueType = fieldValueType;
        }

        public string FieldName { get; }
        public Type FieldValueType { get; }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitFieldToken(this);
        }
        public override string ToString()
        {
            return this.FieldName;
        }
    }
}
