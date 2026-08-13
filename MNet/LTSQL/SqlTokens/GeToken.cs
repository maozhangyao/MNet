using MNet.LTSQL.SqlTokenExtends;

namespace MNet.LTSQL.SqlTokens
{
    public class GeToken : BinaryToken
    {
        internal GeToken(LTSQLToken left, LTSQLToken right) : this(left, right, false)
        { }
        internal GeToken(LTSQLToken left, LTSQLToken right, bool priority) : base(BinaryToken.OPT_GREATER_OR_EQUAL, left, right, typeof(bool), priority)
        { }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitGeToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new GeToken(newLeft, newRight, this.IsPriority);
        }
    }
}
