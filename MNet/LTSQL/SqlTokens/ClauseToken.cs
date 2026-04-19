using System;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// sql 子句
    /// </summary>
    public class ClauseToken : LTSQLToken
    {
        public ClauseToken(string clause, LTSQLToken[] subs) : this(clause, subs, null)
        { }
        public ClauseToken(string clause, LTSQLToken[] subs, Dictionary<string, object> metadata)
        {
            this.ClauseName = clause;
            this.SubClause = subs;
            this.Metadata = metadata == null ? null : new Dictionary<string, object>(metadata);
        }


        /// <summary>
        /// 子句名称： select, from, where, group, order, having 等等
        /// </summary>
        public string ClauseName { get; }
        /// <summary>
        /// 子句内容列表
        /// </summary>
        public LTSQLToken[] SubClause { get; }


        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitClauseToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            LTSQLToken[] arr = null;
            if (this.SubClause != null)
            {
                arr = new LTSQLToken[this.SubClause.Length];
                for (int i = 0; i < this.SubClause.Length; i++)
                {
                    arr[i] = visitor.Visit(this.SubClause[i]);
                }
            }

            return new ClauseToken(this.ClauseName, arr, this.Metadata);
        }
    }
}


