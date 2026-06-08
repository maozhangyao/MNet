using MNet.LTSQL.SqlTokenExtends;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 三元表达式 -> swtich case 
    /// </summary>
    public class SwitchCaseToken : SqlValueToken
    {
        internal SwitchCaseToken(LTSQLToken then, LTSQLToken thenValue,  LTSQLToken thenElse, Type valueType)
            : this(then, thenValue, thenElse, valueType, false)
        {
            this.When = then;
            this.ThenValue = thenValue;
            this.ThenElse = thenElse;
            this.ValueType = valueType;
        }
        internal SwitchCaseToken(LTSQLToken then, LTSQLToken thenValue, LTSQLToken thenElse, Type valueType, bool prior)
        {
            this.When = then;
            this.ThenValue = thenValue;
            this.ThenElse = thenElse;
            this.ValueType = valueType;
            this.IsPriority = prior;
        }

        //THEN 表达式
        public LTSQLToken When { get; }
        //THEN 值(为true时)
        public LTSQLToken ThenValue { get; }
        //THEN ELSE 表达式(为false时)
        public LTSQLToken ThenElse { get; }


        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitSwitchCaseToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            LTSQLToken then = this.When.Visit(visitor);
            LTSQLToken thenValue = this.ThenValue.Visit(visitor);
            LTSQLToken thenElse = this.ThenElse.Visit(visitor);
            return new SwitchCaseToken(then, thenValue, thenElse, this.ValueType, this.IsPriority);
        }
    }
}
