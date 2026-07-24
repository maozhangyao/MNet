namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 逻辑与
    /// </summary>
    public class AndToken : BinaryToken
    { 
        internal AndToken(LTSQLToken left, LTSQLToken right) : base(BinaryToken.OPT_AND, left, right, typeof(bool))
        { }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitAndToken(this);
        }
        protected internal override BinaryToken VisitChildren(LTSQLToken newLeft, LTSQLToken newRight)
        {
            return new AndToken(newLeft, newRight);
        }
    }
}
