using MNet.LTSQL.SqlTokenExtends;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// IN / NOT IN 判断，如： field IN (1,2,3)
    /// </summary>
    public class InToken : BinaryToken, INotable
    {
        internal InToken(LTSQLToken left, LTSQLToken right, bool isNot) : this(left, right, isNot, false)
        { }
        internal InToken(LTSQLToken left, LTSQLToken right, bool isNot, bool priority)
            : base(isNot ? BinaryToken.OPT_NOT_IN : BinaryToken.OPT_IN, left, right, typeof(bool), priority)
        {
            this.IsNot = isNot;
        }

        public bool IsNot { get; }

        public LTSQLToken Not()
        {
            return new InToken(this.Left, this.Right, !this.IsNot, this.IsPriority);
        }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitInToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new InToken(newLeft, newRight, this.IsNot, this.IsPriority);
        }
    }
}
