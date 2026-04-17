using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace MNet.LTSQL.SqlTokens
{
    //一个 sql 硬编码的值
    public class ConstantToken : SqlValueToken
    {
        public ConstantToken(string val, Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            this.Value = val;
            this.ValueType = type;
        }


        public string Value { get; set; }

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
