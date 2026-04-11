using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    //sql 参数
    public class SqlParameterToken : SqlValueToken
    {
        internal SqlParameterToken(string pName, object value, Type valueType)
        {
            this.Value = value;
            this.ParameterName = pName;
            this.ValueType = valueType;
        }


        //值
        public readonly object Value;
        //参数名
        public readonly string ParameterName;


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
