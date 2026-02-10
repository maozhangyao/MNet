namespace MNet.LTSQL.v1.SqlTokens
{
    public class ConditionToken : SQLValueToken
    {
        // = 、 > , < , >= , <= , <> , IN , LIKE , IS NULL , IS NOT NULL , BETWEEN
        public AliasToken CoditionType { get; set; }
        public LTSQLToken Left { get; set; }
        public LTSQLToken Right { get; set; }


        public override string ToSql()
        {
            return "Condition";
        }
    }
}
