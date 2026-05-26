using System;
using System.Linq;
using System.Collections.Generic;
using MNet.LTSQL.SqlTokenExtends;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 一个列表
    /// </summary>
    public class ListToken : SequenceToken, IPriorable
    {
        internal ListToken(IEnumerable<LTSQLToken> fields) : this(fields, false)
        { }
        internal ListToken(IEnumerable<LTSQLToken> fields, bool isPriority) : base(fields?.ToArray())
        {
            this.IsPriority = isPriority;
        }


        public bool IsPriority { get; private set; }


        public IPriorable SetPriority(bool isPriority)
        {
            return new ListToken(this.Tokens, isPriority);
        }
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

            return new ListToken(_news, this.IsPriority);
        }
    }
}