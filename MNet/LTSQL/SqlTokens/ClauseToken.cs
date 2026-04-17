using System;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// sql 子句
    /// </summary>
    public class ClauseToken : LTSQLToken
    {
        public ClauseToken(string clause, LTSQLToken token) : this(clause, token, null)
        { }
        public ClauseToken(string clause, LTSQLToken token, Dictionary<string, object> metadata)
        {
            this.Clause = clause;
            this.Token = token;
            this.Metadata = metadata == null ? null : new Dictionary<string, object>(metadata);
        }


        /// <summary>
        /// 子句名称： select, from, where, group, order, having 等等
        /// </summary>
        public string Clause { get; }
        /// <summary>
        /// 子句内容
        /// </summary>
        public LTSQLToken Token { get; }


        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return base.Visit(visitor);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            return base.VisitChildren(visitor);
        }
    }
}


