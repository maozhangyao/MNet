using System.Runtime.InteropServices;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class WhereToken : LTSQLToken
    {
        public WhereToken()
        { }
        public WhereToken(LTSQLToken condition)
        {
            this.Condition = condition;
        }

        public LTSQLToken Condition { get; set; }

        public override void ToSql(LTSQLTokenContext context)
        {
            context.SQLBuilder.Append("WHERE ");
            this.Condition.ToSql(context);
        }
    }
}
