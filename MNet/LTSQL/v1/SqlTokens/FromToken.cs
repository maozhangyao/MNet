using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class FromToken : LTSQLToken
    {
        //查询语句
        public AliasTable Source { get; set; }
        public override string ToSql()
        {
            return "FROM ";
        }
    }
}
