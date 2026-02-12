namespace MNet.LTSQL.v1.SqlTokens
{
    public class FromJoinToken : FromToken
    {
        public string JoinType { get; set; }
        //from
        public FromToken From { get; set; }
        //连接条件
        public LTSQLToken JoinOn { get; set; }

        public override void ToSql(LTSQLTokenContext context)
        {
            this.From.ToSql(context);

            context.SQLBuilder.AppendLine();
            context.SQLBuilder.Append(JoinType);
            context.SQLBuilder.Append(' ');

            this.Source.ToSql(context);
            context.SQLBuilder.Append(" ON ");

            this.JoinOn.ToSql(context);
        }
    }
}
