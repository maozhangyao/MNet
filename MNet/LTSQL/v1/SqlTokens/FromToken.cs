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

    public class FromJoinToken : LTSQLToken
    {
        public string JoinType { get; set; }

        public AliasTable Source1 { get; set; }
        public AliasTable Source2 { get; set; }
        //连接条件
        public WhereToken JoinOn { get; set; }

        public override string ToSql()
        {
            return $"{JoinType} JOIN ";
        }
    }

    
    public class AliasTable : LTSQLToken
    {
        public LTSQLToken Query { get; set; }
        public AliasToken Alias { get; set; }

        public override string ToSql()
        {
            return "AliasTable";
        }
    }
}
