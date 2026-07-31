using System;

namespace MNet.LTSQL.SqlTokens
{
    public class AddToken : BinaryToken
    {
        internal AddToken(LTSQLToken left, LTSQLToken right, Type typeOfValue, bool priority)
            : base(BinaryToken.OPT_ADD, left, right, typeOfValue, priority)
        { }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitAddToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new AddToken(newLeft, newRight, this.ValueType, this.IsPriority);
        }
    }
}
