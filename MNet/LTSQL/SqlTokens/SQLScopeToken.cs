using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 表示一个作用域，比如子查询作用域，外部无法访问内部
    /// </summary>
    public class SqlScopeToken : LTSQLToken
    {
        internal SqlScopeToken(LTSQLToken inner)
        {
            this.Inner = inner;
        }

        public LTSQLToken Inner { get; private set; }


        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitSQLScopeToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.Inner = this.Inner.Visit(visitor);
            return this;
        }
    }
}
