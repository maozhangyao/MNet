namespace MNet.LTSQL.v1.SqlTokens
{
    public class SelectToken : LTSQLToken
    {
        public override string ToSql()
        {
            return "Select";
        }
    }


}
