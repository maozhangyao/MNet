using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 二元操作
    /// </summary>
    public class BinaryToken : SqlValueToken
    {
        internal BinaryToken()
        { }
        internal BinaryToken(string opt, LTSQLToken left, LTSQLToken right, Type typeOfValue)
        {
            this.Opration = opt;
            this.Left = left;
            this.Right = right;
            this.ValueType = typeOfValue;
        }

        public readonly string Opration;
        //exists 运算没有 left
        public readonly LTSQLToken Left;
        public readonly LTSQLToken Right;

        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return (new[] { this.Left, this.Right }).Where(p => p != null);
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitBinaryToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            LTSQLToken left = this.Left?.Visit(visitor); // 比如 exists 就没有 left
            LTSQLToken right = this.Right?.Visit(visitor);
            return new BinaryToken(this.Opration, left, right, this.ValueType);
        }
    }
}
