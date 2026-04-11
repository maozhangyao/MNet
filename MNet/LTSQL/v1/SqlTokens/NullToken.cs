using System;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 表示一个null值
    /// </summary>
    public class NullToken : ConstantToken
    {
        internal NullToken(Type valueTypeOfNull) : base(null, valueTypeOfNull)
        { }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitNullToken(this);
        }

        public static NullToken Create(Type valueTypeOfNull, DbType db)
        {
            return new NullToken(valueTypeOfNull)
            {
                Value = DbUtils.ToSqlPart(null, db)
            };
        }
    }
}
