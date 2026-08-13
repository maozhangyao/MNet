using MNet.LTSQL.SqlTokenExtends;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// IN / NOT IN 判断，如： field IN (1,2,3)
    /// </summary>
    public class InToken : BinaryToken
    {
        internal InToken(LTSQLToken left, LTSQLToken right, bool priority)
            : base(BinaryToken.OPT_IN, left, right, typeof(bool), priority)
        {
        }


        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitInToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new InToken(newLeft, newRight, this.IsPriority);
        }
    }
}
