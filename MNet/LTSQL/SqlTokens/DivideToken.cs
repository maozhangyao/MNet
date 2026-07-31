using System;

namespace MNet.LTSQL.SqlTokens
{
    public class DivideToken : BinaryToken
    {
        internal DivideToken(LTSQLToken left, LTSQLToken right, Type typeOfValue, bool priority)
            : base(BinaryToken.OPT_DIVIDE, left, right, typeOfValue, priority)
        { }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitDivideToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new DivideToken(newLeft, newRight, this.ValueType, this.IsPriority);
        }
    }
}
