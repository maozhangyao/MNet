using System;
using System.Linq;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 一组字段列表，如：select,order,group等等中的字段列表
    /// </summary>
    public class FieldListToken : SequenceToken
    {
        public FieldListToken(IEnumerable<LTSQLToken> fields) : base(fields?.ToArray())
        { }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitFieldListToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            if (this.Tokens == null)
                return this;

            int len = this.Tokens.Length;
            LTSQLToken[] _news = new LTSQLToken[len];
            for (int i = 0; i < len; i++)
                _news[i] = this.Tokens[i].Visit(visitor);

            return new FieldListToken(_news);
        }
    }
}