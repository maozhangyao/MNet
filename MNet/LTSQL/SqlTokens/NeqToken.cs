namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 不等于
    /// </summary>
    public class NeqToken : BinaryToken
    {
        internal NeqToken(LTSQLToken left, LTSQLToken right) : base(BinaryToken.OPT_NOT_EQUAL, left, right, typeof(bool))
        { }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitNeqToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new NeqToken(newLeft, newRight);
        }
    }
}
