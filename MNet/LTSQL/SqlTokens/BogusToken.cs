using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 伪SQL概念，无法直接转换成SQL
    /// </summary>
    public class BogusToken : ValueToken
    {
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return this;
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            return this;
        }
    }
}
