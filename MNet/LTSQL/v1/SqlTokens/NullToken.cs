using System;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 表示一个null值
    /// </summary>
    public class NullToken : ConstantToken
    {
        public NullToken(Type valueTypeOfNull) : base("NULL", valueTypeOfNull)
        { }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitNullToken(this);
        }
    }
}
