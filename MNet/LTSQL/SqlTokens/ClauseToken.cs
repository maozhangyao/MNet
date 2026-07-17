using MNet.Utils;
using System;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// sql 子句
    /// </summary>
    public class ClauseToken : LTSQLToken
    {
        public ClauseToken(string clause, LTSQLToken[] subs)
        { }


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
        protected internal virtual ClauseToken VisitChildren(LTSQLToken[] newSubClause)
        {
            return new ClauseToken(this.ClauseName, newSubClause);
        }
        protected internal virtual LTSQLToken[] VisitChildrenCore(LTSQLTokenVisitor visitor)
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
            return arr;
        }

        protected internal sealed override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            LTSQLToken[] arr = this.VisitChildrenCore(visitor);
            return VisitChildren(arr);
        }
        public override string ToString()
        {
            if (this.SubClause == null || this.SubClause.Length < 1)
                return this.ClauseName;

            return this.ClauseName + " " + this.SubClause.JoinAsString(" ");
        }
    }


    public class FromClauseToken : ClauseToken
    {
        public FromClauseToken(LTSQLToken src) : base("FROM", new[] { src })
        { }

        protected internal override ClauseToken VisitChildren(LTSQLToken[] newSubClause)
        {
            return new FromClauseToken(newSubClause[0]);
        }
    }

    public class WhereClauseToken : ClauseToken
    {
        public WhereClauseToken(LTSQLToken condition) : base("WHERE", new[] { condition })
        { }

        protected internal override ClauseToken VisitChildren(LTSQLToken[] newSubClause)
        {
            return new WhereClauseToken(newSubClause[0]);
        }
    }

    public class OrderByClauseToken : ClauseToken
    {
        public OrderByClauseToken(LTSQLToken[] orderList) : base("ORDER BY", new[] { LTSQLTokenFactory.CreateListToken(orderList) })
        { }

        protected internal override ClauseToken VisitChildren(LTSQLToken[] newSubClause)
        {
            return new OrderByClauseToken(newSubClause);
        }
    }

    public class GroupClauseToken : ClauseToken
    {
        public GroupClauseToken(LTSQLToken[] groupList) : base("GROUP BY", new []{ LTSQLTokenFactory.CreateListToken(groupList) })
        { }

        protected internal override ClauseToken VisitChildren(LTSQLToken[] newSubClause)
        {
            return new GroupClauseToken(newSubClause);
        }
    }

    public class HavingClauseToken : ClauseToken
    {
        public HavingClauseToken(LTSQLToken condition) : base("HAVAING", new[] { condition })
        { }

        protected internal override ClauseToken VisitChildren(LTSQLToken[] newSubClause)
        {
            return new HavingClauseToken(newSubClause[0]);
        }
    }

    public class TopClauseToken : ClauseToken
    {
        public TopClauseToken(LTSQLToken take) : base("TOP", new[] { take } )
        { }

        protected internal override ClauseToken VisitChildren(LTSQLToken[] newSubClause)
        {
            return new TopClauseToken(newSubClause[0]);
        }
    }

    public class DistinctToken : ClauseToken
    {
        public DistinctToken() : base("DISTINCT", null)
        { }

        protected internal override ClauseToken VisitChildren(LTSQLToken[] newSubClause)
        {
            return this;//对象直接重用
        }
    }
}


