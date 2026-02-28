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
        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return Array.Empty<LTSQLToken>();
        }
        public sealed override void ToSql(LTSQLTokenContext context)
        {
            throw new NotImplementedException("bogus token 无法直接转成sql.");
        }

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
