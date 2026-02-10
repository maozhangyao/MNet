namespace MNet.LTSQL.v1.SqlTokens
{
    public class WhereToken : LTSQLToken
    {
        public WhereToken()
        {
        }

        public LTSQLToken Condition { get; set; }

        public override string ToSql()
        {
            return "Where";
        }
    }
}
