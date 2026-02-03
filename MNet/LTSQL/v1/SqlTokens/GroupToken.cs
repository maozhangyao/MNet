using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class GroupToken : LTSQLToken
    {
        public List<LTSQLToken> GroupByItems { get; set; }

        public override string ToSql()
        {
            return "Group";
        }
    }


}
