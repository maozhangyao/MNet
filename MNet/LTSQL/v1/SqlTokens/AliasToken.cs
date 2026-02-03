namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// SQL 命名
    /// </summary>
    public class AliasToken : LTSQLToken
    {
        public string Alias { get; set; }
        public override string ToSql()
        {
            return Alias;
        }
    }
}
