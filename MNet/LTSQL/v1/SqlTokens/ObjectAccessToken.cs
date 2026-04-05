using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    // sql 对象的访问
    public class ObjectAccessToken : SqlValueToken
    {
        public ObjectAccessToken() { }
        public ObjectAccessToken(LTSQLToken owner, string filed) 
        {
            this.Owner = owner;
            this.Field = filed;
        }


        //表名
        public LTSQLToken Owner { get; set; }
        //表字段， 注意翻译时需要做关键词转义
        public string Field { get; set; }



        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { this.Owner };
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitObjectAccessToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.Owner = this.Owner.Visit(visitor);
            return this;
        }
    }
}
