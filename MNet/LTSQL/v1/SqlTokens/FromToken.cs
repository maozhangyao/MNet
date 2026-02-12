using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class FromToken : LTSQLToken
    {
        public Type SourceType { get; set; }
        //查询语句
        public AliasTable Source { get; set; }
        public override void ToSql(LTSQLTokenContext context)
        {
            context.SQLBuilder.Append("FROM ");
            this.Source.ToSql(context);
        }
    }
}
