namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 等于
    /// </summary>
    public class EqToken : BinaryToken
    {
        internal EqToken(LTSQLToken left, LTSQLToken right) : base(BinaryToken.OPT_EQUAL, left, right, typeof(bool))
        { }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitEqToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new EqToken(newLeft, newRight);
        }
    }
}
