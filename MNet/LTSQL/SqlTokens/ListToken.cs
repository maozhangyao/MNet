using System;
using System.Linq;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 一个列表
    /// </summary>
    public class ListToken : SequenceToken
    {
        public ListToken(IEnumerable<LTSQLToken> fields) : base(fields?.ToArray())
        { }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitListToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            if (this.Tokens == null)
                return this;

            int len = this.Tokens.Length;
            LTSQLToken[] _news = new LTSQLToken[len];
            for (int i = 0; i < len; i++)
                _news[i] = this.Tokens[i].Visit(visitor);

            return new ListToken(_news);
        }
    }
}