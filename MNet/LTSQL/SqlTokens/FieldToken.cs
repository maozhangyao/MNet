using System;

namespace MNet.LTSQL.SqlTokens
{
    public class FieldToken : LTSQLToken
    {
        internal FieldToken(string fieldName, string originFieldName, Type fieldValueType)
        {
            this.FieldName = fieldName;
            this.OrginFieldName = originFieldName;
            this.FieldValueType = fieldValueType;
        }

        /// <summary>
        /// 映射字段，数据库表中的字段名
        /// </summary>
        public string FieldName { get; }
        /// <summary>
        /// 源字段，类中的字段或者属性名
        /// </summary>
        public string? OrginFieldName { get; }
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
