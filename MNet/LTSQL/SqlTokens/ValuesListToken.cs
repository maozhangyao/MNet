using System.Linq;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 表示一组值列表，如：(1,2,3,4,5,6)
    /// </summary>
    public class ValuesListToken : SequenceToken
    {
        public ValuesListToken(IEnumerable<LTSQLToken> values) : base(values?.ToArray())
        { }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitValuesListToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            if (this.Tokens == null)
                return this;

            int len = this.Tokens.Length;
            LTSQLToken[] _news = new LTSQLToken[len];
            for (int i = 0; i < len; i++)
                _news[i] = this.Tokens[i].Visit(visitor);

            return new ValuesListToken(_news);
        }
    }
}