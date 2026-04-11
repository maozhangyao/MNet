using System;
using System.Linq;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class FunctionCallToken : SqlValueToken
    {
        public FunctionCallToken(LTSQLToken fObj, LTSQLToken[] args, Type typeOfValue)
        {
            this.FunctionObject = fObj;
            this.Parameters = args ?? new LTSQLToken[0];
            this.ValueType = typeOfValue;
        }

        public readonly LTSQLToken FunctionObject;
        public readonly LTSQLToken[] Parameters;

        public readonly LTSQLToken Call;

        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new LTSQLToken[] { this.FunctionObject }.Concat(this.Parameters);
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
