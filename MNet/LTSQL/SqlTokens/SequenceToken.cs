using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MNet.LTSQL.SqlTokenExtends;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 顺序存储token
    /// </summary>
    public class SequenceToken : LTSQLToken, IContainerable
    {
        private SequenceToken(params LTSQLToken[] tokens)
        {
            this.Tokens = tokens;
        }
        private SequenceToken(IEnumerable<LTSQLToken> tokens)
        {
            this.Tokens = tokens?.ToArray();
        }


        public readonly LTSQLToken[] Tokens;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<LTSQLToken> GetEnumerator()
        {
            return (this.Tokens as IEnumerable<LTSQLToken>)?.GetEnumerator();
        }


        public static SequenceToken Create(params LTSQLToken[] tokens)
        {
            return new SequenceToken(tokens);
        }
        public static SequenceToken CreateWithJoin(IEnumerable<LTSQLToken> tokens,  LTSQLToken separator)
        {
            if (tokens == null)
                return new SequenceToken(null);
            if (separator == null)
                throw new ArgumentNullException(nameof(separator));

            List<LTSQLToken> list = new List<LTSQLToken>();
            foreach(LTSQLToken token in tokens)
            {
                list.Add(token);
                list.Add(separator);
            }

            list.RemoveAt(list.Count - 1);
            return new SequenceToken(list);
        }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return base.Visit(visitor);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            if (this.Tokens == null)
                return this;

            int len = this.Tokens.Length;
            LTSQLToken[] _news = new LTSQLToken[len];
            for (int i = 0; i < len; i++)
                _news[i] = this.Tokens[i].Visit(visitor);

            return new SequenceToken(_news);
        }
    }
}
