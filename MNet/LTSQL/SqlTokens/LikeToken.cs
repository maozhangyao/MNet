using MNet.LTSQL.SqlTokenExtends;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// LIKE / NOT LIKE 模糊匹配
    /// </summary>
    public class LikeToken : BinaryToken, INotable
    {
        internal LikeToken(LTSQLToken left, LTSQLToken right, bool isNot) : this(left, right, isNot, false)
        { }
        internal LikeToken(LTSQLToken left, LTSQLToken right, bool isNot, bool priority)
            : base(isNot ? BinaryToken.OPT_NOT_LIKE : BinaryToken.OPT_LIKE, left, right, typeof(bool), priority)
        {
            this.IsNot = isNot;
        }

        public bool IsNot { get; }

        public LTSQLToken Not()
        {
            return new LikeToken(this.Left, this.Right, !this.IsNot, this.IsPriority);
        }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitLikeToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new LikeToken(newLeft, newRight, this.IsNot, this.IsPriority);
        }
    }
}
