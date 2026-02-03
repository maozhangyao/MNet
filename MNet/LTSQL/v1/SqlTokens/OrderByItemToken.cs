namespace MNet.LTSQL.v1.SqlTokens
{
    public class OrderByItemToken : LTSQLToken
    {
        public LTSQLToken Item { get; set; }
        public bool IsAsc { get; set; } = true;
        public override string ToSql()
        {
            return "OrderByItem";
        }
    }
}
