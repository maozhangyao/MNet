using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class GroupToken : LTSQLToken
    {
        public List<LTSQLToken> GroupByItems { get; set; }

        public override void ToSql(LTSQLTokenContext context)
        {
            context.SQLBuilder.Append("GROUP BY ");
            bool comma = false;
            foreach (LTSQLToken item in GroupByItems)
            {
                if (comma)
                    context.SQLBuilder.Append(',');
                comma = true;
                item.ToSql(context);
            }
        }
    }


}
