using System;

namespace MNet.LTSQL.v1.SqlTokens
{
    // sql 对象的访问
    public class ObjectAccessToken : SQLValueToken
    {
        public ObjectAccessToken() { }
        public ObjectAccessToken(LTSQLToken owner, string filed) 
        {
            this.Owner = owner;
            this.Field = filed;
        }


        //表名
        public LTSQLToken Owner { get; set; }
        //表字段
        public string Field { get; set; }

        public override void ToSql(LTSQLTokenContext context)
        {
            this.Owner.ToSql(context);
            context.SQLBuilder.Append('.');
            context.SQLBuilder.Append(this.Field);
        }
    }
}
