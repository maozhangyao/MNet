using MNet.LTSQL.SqlTokenExtends;
using System;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 运算优先级tokien，用优先级运算符包括的token(括号括起来的部分), 表示优先求值
    /// 使用IPriorable接口的SetPriority方法来设置token的优先级，而不是直接使用PriorityCalcToken类，因为有些token可能不需要优先级运算符来表示优先求值
    /// </summary>
    public class PriorityCalcToken : LTSQLToken
    {
        internal PriorityCalcToken(LTSQLToken inner)
        {
            this.Value = inner;
            if (inner is PriorityCalcToken priority)
                inner = priority.Value;

            //this.ValueType = (inner as SqlValueToken)?.ValueType;
        }


        public readonly LTSQLToken Value;
        
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitPriorityCalcToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            return new PriorityCalcToken(this.Value.Visit(visitor));
        }
    }
}
