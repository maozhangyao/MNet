using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    //一个 sql 硬编码的值
    public class ConstantToken : SqlValueToken
    {
        public ConstantToken(string val)
        {
            this.Value = val;
        }
        public ConstantToken(string val, Type type)
        {
            this.Value = val;
            this.ValueType = type;
        }


        public string Value { get; set; }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return Array.Empty<LTSQLToken>();
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

    /// <summary>
    /// 表示一个null值
    /// </summary>
    public class NullToken : ConstantToken
    {
        public NullToken(Type valueTypeOfNull) : base("NULL", valueTypeOfNull)
        { }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitNullToken(this);
        }
    }
}
