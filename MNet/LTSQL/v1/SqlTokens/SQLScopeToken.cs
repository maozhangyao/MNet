using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 表示一个作用域，比如子查询作用域，外部无法访问内部
    /// </summary>
    public class SQLScopeToken : SQLValueToken
    {
        public SQLScopeToken(LTSQLToken inner) : this(inner, (inner as SQLValueToken)?.ValueType)
        { }

        public SQLScopeToken(LTSQLToken inner,  Type valueType)
        {
            this.Inner = inner;
            this.ValueType = valueType;
        }

        public LTSQLToken Inner { get; }

        public override void ToSql(LTSQLTokenContext context)
        {
            context.SQLBuilder.Append('(');
            this.Inner.ToSql(context);
            context.SQLBuilder.Append(')');
        }
    }
}
