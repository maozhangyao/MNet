namespace MNet.LTSQL.v1.SqlTokens
{
    public class FieldAccessToken : LTSQLToken
    {
        //表名
        public AliasToken Owner { get; set; }
        //表字段
        public AliasToken Field { get; set; }

        public override string ToSql()
        {
            return "Field";
        }
    }

    
}
