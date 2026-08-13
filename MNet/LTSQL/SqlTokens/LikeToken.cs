using MNet.LTSQL.SqlTokenExtends;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// LIKE / NOT LIKE 模糊匹配
    /// </summary>
    public class LikeToken : BinaryToken
    {
        internal LikeToken(LTSQLToken left, LTSQLToken right, bool priority)
            : base(BinaryToken.OPT_LIKE, left, right, typeof(bool), priority)
        { }


        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitLikeToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new LikeToken(newLeft, newRight, this.IsPriority);
        }
    }
}
