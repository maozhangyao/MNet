using System;

namespace MNet.LTSQL.v1.SqlTokens
{
    // sql 对象的访问
    public class ObjectAccessToken : SQLValueToken
    {
        public ObjectAccessToken() { }
        public ObjectAccessToken(LTSQLToken owner, LTSQLToken filed) 
        {
            this.Owner = owner;
            this.Field = filed;
        }


        //表名
        public LTSQLToken Owner { get; set; }
        //表字段
        public LTSQLToken Field { get; set; }

        public override string ToSql()
        {
            return "Field";
        }
    }
}
