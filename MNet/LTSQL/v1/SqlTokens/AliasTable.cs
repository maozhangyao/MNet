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

        public override void ToSql(LTSQLTokenContext context)
        {
            this.Query.ToSql(context);
            context.SQLBuilder.Append(' ');
            context.SQLBuilder.Append(this.Alias);
        }
    }
}
