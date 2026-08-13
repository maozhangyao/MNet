using MNet.LTSQL.SqlTokenExtends;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// IS / IS NOT 判断，如： field IS NULL
    /// </summary>
    public class IsToken : BinaryToken
    {
        internal IsToken(LTSQLToken left, LTSQLToken right, bool priority)
            : base(BinaryToken.OPT_IS, left, right, typeof(bool), priority)
        {
        }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitIsToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new IsToken(newLeft, newRight, this.IsPriority);
        }
    }
}
