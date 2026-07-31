using System;

namespace MNet.LTSQL.SqlTokens
{
    public class MultiplyToken : BinaryToken
    {
        internal MultiplyToken(LTSQLToken left, LTSQLToken right, Type typeOfValue, bool priority)
            : base(BinaryToken.OPT_MULTIPLY, left, right, typeOfValue, priority)
        { }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitMultiplyToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new MultiplyToken(newLeft, newRight, this.ValueType, this.IsPriority);
        }
    }
}
