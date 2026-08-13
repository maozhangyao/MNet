using MNet.LTSQL.SqlTokenExtends;

namespace MNet.LTSQL.SqlTokens
{
    public class LtToken : BinaryToken
    {
        internal LtToken(LTSQLToken left, LTSQLToken right) : this(left, right, false)
        { }
        internal LtToken(LTSQLToken left, LTSQLToken right, bool priority) : base(BinaryToken.OPT_LESS, left, right, typeof(bool), priority)
        { }


        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitLtToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new LtToken(newLeft, newRight, this.IsPriority);
        }
    }
}
