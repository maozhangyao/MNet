using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 表示一个作用域，比如子查询作用域，外部无法访问内部
    /// </summary>
    public class SqlScopeToken : SqlValueToken
    {
        public SqlScopeToken(LTSQLToken inner) : this(inner, (inner as SqlValueToken)?.ValueType)
        { }

        public SqlScopeToken(LTSQLToken inner,  Type valueType)
        {
            this.Inner = inner;
            this.ValueType = valueType;
        }

        public LTSQLToken Inner { get; private set; }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { this.Inner };
        }
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
