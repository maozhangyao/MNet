namespace MNet.LTSQL.SqlTokens
{
    public class OrToken : BinaryToken
    {
        internal OrToken(LTSQLToken left, LTSQLToken right) : base(BinaryToken.OPT_OR, left, right, typeof(bool))
        { }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitOrToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new OrToken(newLeft, newRight);
        }
    }
}
