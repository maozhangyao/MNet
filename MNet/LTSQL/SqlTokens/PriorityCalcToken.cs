using MNet.LTSQL.SqlTokenExtends;
using System;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 运算优先级tokien，用优先级运算符包括的token(括号括起来的部分), 表示优先求值
    /// </summary>
    public class PriorityCalcToken : SqlValueToken, INotable
    {
        internal PriorityCalcToken(LTSQLToken inner)
        {
            this.Value = inner;
            if(inner is PriorityCalcToken priority)
                inner = priority.Value;

            this.ValueType = (inner as SqlValueToken)?.ValueType;
        }


        public readonly LTSQLToken Value;
        public bool IsNot => (Value is INotable not) ? not.IsNot : throw new Exception("内部节点类型不支持取反操作");


        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitPriorityCalcToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            return new PriorityCalcToken(this.Value.Visit(visitor));
        }

        public LTSQLToken Not()
        {
            if (this.Value is null)
                throw new Exception("优先级运算符内部节点为null, 无法取反操作。");
            if (this.Value is INotable notable)
                return new PriorityCalcToken(notable.Not());

            throw new Exception($"该节点类型不支持取反操作：{this.Value?.ToString()}");
        }
    }
}
