using System;
using System.Linq;
using System.Collections.Generic;
using MNet.LTSQL.SqlTokenExtends;

namespace MNet.LTSQL.SqlTokens
{
    public class FunctionCallToken : SqlValueToken, INotable
    {
        internal FunctionCallToken(LTSQLToken fObj, LTSQLToken[] args, Type typeOfValue) : this(fObj, args, typeOfValue, false)
        { }
        private FunctionCallToken(LTSQLToken fObj, LTSQLToken[] args, Type typeOfValue, bool isNot)
        {
            this.IsNot = isNot;
            this.FunctionObject = fObj;
            this.Parameters = args ?? new LTSQLToken[0];
            this.ValueType = typeOfValue;
        }

        public readonly LTSQLToken FunctionObject;
        public readonly LTSQLToken[] Parameters;

        public bool IsNot { get; }

        public LTSQLToken Not()
        {
            return new FunctionCallToken(this.FunctionObject, this.Parameters, this.ValueType, !this.IsNot);
        }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitFunctionCallToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            LTSQLToken fObj = this.FunctionObject.Visit(visitor);
            LTSQLToken[] args = new LTSQLToken[this.Parameters.Length];
            for (int i = 0; i < this.Parameters.Length; i++)
                args[i] = this.Parameters[i].Visit(visitor);

            return new FunctionCallToken(fObj, args, this.ValueType);
        }
    }
}
