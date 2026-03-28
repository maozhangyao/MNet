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
        public BinaryToken()
        { }
        public BinaryToken(string opt, LTSQLToken left, LTSQLToken right, Type typeOfValue)
        {
            this.Opration = opt;
            this.Left = left;
            this.Right = right;
            this.ValueType = typeOfValue;
        }

        public string Opration { get; set; }
        //exists 运算没有 left
        public LTSQLToken Left { get; set; }
        public LTSQLToken Right { get; set; }


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
            this.Left = this.Left?.Visit(visitor); // 比如 exists 就没有 left
            this.Right = this.Right?.Visit(visitor);
            return this;
        }

        public static BinaryToken CreateAdd(LTSQLToken left, LTSQLToken right, Type typeOfValue)
        {
            return new BinaryToken("+", left, right, typeOfValue);
        }
        public static BinaryToken CreateSubtract(LTSQLToken left, LTSQLToken right, Type typeOfValue)
        {
            return new BinaryToken("-", left, right, typeOfValue);
        }
        public static BinaryToken CreateDivide(LTSQLToken left, LTSQLToken right, Type typeOfValue)
        {
            return new BinaryToken("/", left, right, typeOfValue);
        }
        public static BinaryToken CreateMultiply(LTSQLToken left, LTSQLToken right, Type typeOfValue)
        {
            return new BinaryToken("*", left, right, typeOfValue);
        }
    }


}
