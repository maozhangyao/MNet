using MNet.LTSQL.SqlTokenExtends;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 等于
    /// </summary>
    public class EqToken : BinaryToken
    {
        internal EqToken(LTSQLToken left, LTSQLToken right) : this(left, right, false)
        { }
        internal EqToken(LTSQLToken left, LTSQLToken right, bool priority) : base(BinaryToken.OPT_EQUAL, left, right, typeof(bool), priority)
        { }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitEqToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new EqToken(newLeft, newRight, this.IsPriority);
        }
    }
}
