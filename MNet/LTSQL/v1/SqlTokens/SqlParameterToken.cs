using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    //sql 参数
    public class SqlParameterToken : SqlValueToken
    {
        public SqlParameterToken() 
        { }
        public SqlParameterToken(string pName, object value)
        {
            this.Value = value;
            this.ParameterName = pName;
        }


        //值
        public object Value { get; set; }
        //参数名
        public string ParameterName { get; set; }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return Array.Empty<LTSQLToken>();
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitSqlParameterToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            return this;
        }
    }
}
