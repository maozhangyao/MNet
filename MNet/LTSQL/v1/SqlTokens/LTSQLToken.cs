using System;

namespace MNet.LTSQL.v1.SqlTokens
{
    //表示 sql 结构的一部分
    public abstract class LTSQLToken
    {
        public abstract void ToSql(LTSQLTokenContext context);
    }
}
