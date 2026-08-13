using MNet.LTSQL.SqlTokenExtends;

namespace MNet.LTSQL.SqlTokens
{
    public class GtToken : BinaryToken
    {
        internal GtToken(LTSQLToken left, LTSQLToken right) : this(left, right, false)
        { }
        internal GtToken(LTSQLToken left, LTSQLToken right, bool priority) : base(BinaryToken.OPT_GREATER, left, right, typeof(bool), priority)
        { }


        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitGtToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new GtToken(newLeft, newRight, this.IsPriority);
        }
    }
}
