namespace MNet.LTSQL.v1.SqlTokens
{
    public class ConditionToken : SQLValueToken
    {
        // AND , RO  = , > , < , >= , <= , <> , IN , LIKE , IS NULL , IS NOT NULL , BETWEEN
        public ConditionToken() 
        {
            this.ValueType = typeof(bool);
        }
        public ConditionToken(LTSQLToken left, LTSQLToken right, string opt)
        {
            this.Left = left;
            this.Right = right;
            this.ConditionType = opt;
            this.ValueType = typeof(bool);
        }


        public string ConditionType { get; set; }
        public LTSQLToken Left { get; set; }
        public LTSQLToken Right { get; set; }


        public override void ToSql(LTSQLTokenContext context)
        {
            this.Left.ToSql(context);
            context.SQLBuilder.Append(' ');
            context.SQLBuilder.Append(ConditionType);
            context.SQLBuilder.Append(' ');
            this.Right.ToSql(context);
        }
    }
}
