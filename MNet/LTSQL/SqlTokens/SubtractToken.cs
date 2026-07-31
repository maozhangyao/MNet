using System;

namespace MNet.LTSQL.SqlTokens
{
    public class SubtractToken : BinaryToken
    {
        internal SubtractToken(LTSQLToken left, LTSQLToken right, Type typeOfValue, bool priority)
            : base(BinaryToken.OPT_SUBTRACT, left, right, typeOfValue, priority)
        { }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitSubtractToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new SubtractToken(newLeft, newRight, this.ValueType, this.IsPriority);
        }
    }
}
