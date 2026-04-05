using MNet.LTSQL.v1.SqlTokenExtends;
using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 运算优先级tokien，用优先级运算符包括的token(括号括起来的部分), 表示优先求值，所以必须包裹一个sqlvalue
    /// </summary>
    public class PriorityCalcToken : SqlValueToken, INotable
    {
        public PriorityCalcToken(SqlValueToken inner)
        {
            this.Value = inner;
            this.ValueType = inner?.ValueType;
        }


        public readonly SqlValueToken Value;

        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { this.Value };
        }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitPriorityCalcToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            return new PriorityCalcToken(this.Value.Visit(visitor) as SqlValueToken);
        }

        public LTSQLToken Not()
        {
            return (this.Value as INotable)?.Not();
        }
    }
}
