namespace MNet.LTSQL.v1.SqlTokens
{
    public class FromJoinToken : FromToken
    {
        public string JoinType { get; set; }
        //from
        public FromToken From { get; set; }
        //连接条件
        public WhereToken JoinOn { get; set; }

        public override string ToSql()
        {
            return $"{JoinType} JOIN ";
        }
    }
}
