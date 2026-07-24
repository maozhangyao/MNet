namespace MNet.LTSQL.SqlTokens
{
    public class GeToken : BinaryToken
    {
        internal GeToken(LTSQLToken left, LTSQLToken right) : base(BinaryToken.OPT_GREATER_OR_EQUAL, left, right, typeof(bool))
        { }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitGeToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new GeToken(newLeft, newRight);
        }
    }
}
