using MNet.LTSQL.SqlTokenExtends;

namespace MNet.LTSQL.SqlTokens
{
    public class LeToken : BinaryToken
    {
        internal LeToken(LTSQLToken left, LTSQLToken right) : this(left, right, false)
        { }
        internal LeToken(LTSQLToken left, LTSQLToken right, bool priority) : base(BinaryToken.OPT_LESS_OR_EQUAL, left, right, typeof(bool), priority)
        { }


        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitLeToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new LeToken(newLeft, newRight, this.IsPriority);
        }
    }
}
