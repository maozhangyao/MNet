using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 伪SQL概念，无法直接转换成SQL
    /// </summary>
    public class BogusToken : ValueToken
    {
        public sealed override void ToSql(LTSQLTokenContext context)
        {
            throw new NotImplementedException("bogus token 无法直接转成sql.");
        }
    }
}
