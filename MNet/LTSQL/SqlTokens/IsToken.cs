using MNet.LTSQL.SqlTokenExtends;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// IS / IS NOT 判断，如： field IS NULL
    /// </summary>
    public class IsToken : BinaryToken, INotable
    {
        internal IsToken(LTSQLToken left, LTSQLToken right, bool isNot) : this(left, right, isNot, false)
        { }
        internal IsToken(LTSQLToken left, LTSQLToken right, bool isNot, bool priority)
            : base(isNot ? BinaryToken.OPT_IS_NOT : BinaryToken.OPT_IS, left, right, typeof(bool), priority)
        {
            this.IsNot = isNot;
        }

        public bool IsNot { get; }

        public LTSQLToken Not()
        {
            return new IsToken(this.Left, this.Right, !this.IsNot, this.IsPriority);
        }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitIsToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new IsToken(newLeft, newRight, this.IsNot, this.IsPriority);
        }
    }
}
