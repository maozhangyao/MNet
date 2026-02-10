namespace MNet.LTSQL.v1.SqlTokens
{
    public class AliasTable : LTSQLToken
    {
        public AliasTable() 
        { }
        public AliasTable(string alias, LTSQLToken sqlObj)
        {
            this.Alias = alias;
            this.Query = sqlObj;
        }

        public LTSQLToken Query { get; set; }
        public string Alias { get; set; }

        public override string ToSql()
        {
            return "AliasTable";
        }
    }
}
