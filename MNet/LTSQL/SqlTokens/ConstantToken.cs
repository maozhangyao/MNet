using MNet.LTSQL.SqlTokenExtends;
using System;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
    //一个 sql 硬编码的值
    public class ConstantToken : SqlValueToken
    {
        internal ConstantToken(string val, Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            this.Value = val;
            this.ValueType = type;
        }


        public string Value { get; set; }

        public override IPriorable SetPriority(bool isPriority)
        {
            return new ConstantToken(this.Value, this.ValueType) { IsPriority = isPriority };
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitConstantToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            return this;
        }
    }
}
