namespace MNet.LTSQL.v1.SqlTokens
{
    public class OrderByItemToken : LTSQLToken
    {
        public LTSQLToken Item { get; set; }
        public bool IsAsc { get; set; } = true;
        public override void ToSql(LTSQLTokenContext context)
        {
            this.Item.ToSql(context);
            context.SQLBuilder.Append(' ');
            if (!this.IsAsc)
                context.SQLBuilder.Append("desc");
        }
    }
}
