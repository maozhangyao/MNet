using MNet.LTSQL.SqlTokenExtends;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 不等于
    /// </summary>
    public class NeqToken : BinaryToken
    {
        internal NeqToken(LTSQLToken left, LTSQLToken right) : this(left, right, false)
        { }
        internal NeqToken(LTSQLToken left, LTSQLToken right, bool priority) : base(BinaryToken.OPT_NOT_EQUAL, left, right, typeof(bool), priority)
        { }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitNeqToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new NeqToken(newLeft, newRight, this.IsPriority);
        }
    }
}
