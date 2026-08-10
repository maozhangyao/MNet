using MNet.LTSQL.SqlTokens;
using System;

namespace MNet.LTSQL.Objects
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
    }
}
