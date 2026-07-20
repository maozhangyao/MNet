using MNet.LTSQL.SqlTokenExtends;
using MNet.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// sql 子句
    /// </summary>
    public class ClauseToken : LTSQLToken
    {
        internal ClauseToken(string clause, LTSQLToken[] subs)
        {
            this.Clause = clause;
            this.SubClause = subs;
        }


        /// <summary>
        /// 子句名称： select, from, where, group, order, having 等等
        /// </summary>
        public string Clause { get; }
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
            return new ClauseToken(this.Clause, newSubClause);
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
                return this.Clause;

            return this.Clause + " " + this.SubClause.JoinAsString(" ");
        }
    }


    public class FromClauseToken : ClauseToken
    {
        internal FromClauseToken(LTSQLToken src) : base("FROM", new[] { src })
        { }

        protected internal override ClauseToken VisitChildren(LTSQLToken[] newSubClause)
        {
            return new FromClauseToken(newSubClause[0]);
        }
    }

    public class WhereClauseToken : ClauseToken
    {
        internal WhereClauseToken(LTSQLToken condition) : base("WHERE", new[] { condition })
        { }

        protected internal override ClauseToken VisitChildren(LTSQLToken[] newSubClause)
        {
            return new WhereClauseToken(newSubClause[0]);
        }
    }

    public class OrderByClauseToken : ClauseToken
    {
        internal OrderByClauseToken(LTSQLToken[] orderList) : base("ORDER BY", new[] { LTSQLTokenFactory.CreateListToken(orderList) })
        { }

        protected internal override ClauseToken VisitChildren(LTSQLToken[] newSubClause)
        {
            if (newSubClause[0] is IContainerable container)
                return new OrderByClauseToken(container.ToArray());
            else
                return null;
        }
    }

    public class GroupClauseToken : ClauseToken
    {
        internal GroupClauseToken(LTSQLToken[] groupList) : base("GROUP BY", new[] { LTSQLTokenFactory.CreateListToken(groupList) })
        { }

        protected internal override ClauseToken VisitChildren(LTSQLToken[] newSubClause)
        {
            if (newSubClause[0] is IContainerable container)
                return new GroupClauseToken(container.ToArray());
            else
                return null;
        }
    }

    public class HavingClauseToken : ClauseToken
    {
        internal HavingClauseToken(LTSQLToken condition) : base("HAVING", new[] { condition })
        { }

        protected internal override ClauseToken VisitChildren(LTSQLToken[] newSubClause)
        {
            return new HavingClauseToken(newSubClause[0]);
        }
    }

    public class TopClauseToken : ClauseToken
    {
        internal TopClauseToken(LTSQLToken take) : base("TOP", new[] { take })
        { }

        protected internal override ClauseToken VisitChildren(LTSQLToken[] newSubClause)
        {
            return new TopClauseToken(newSubClause[0]);
        }
    }

    public class DistinctToken : ClauseToken
    {
        internal DistinctToken() : base("DISTINCT", null)
        { }

        protected internal override ClauseToken VisitChildren(LTSQLToken[] newSubClause)
        {
            return this;//对象直接重用
        }
    }
}


