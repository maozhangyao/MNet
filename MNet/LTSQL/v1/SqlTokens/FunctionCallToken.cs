using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class FunctionCallToken : SqlValueToken
    {
        public FunctionCallToken(LTSQLToken call, Type typeOfValue)
        {
            this.Call = call;
            this.ValueType = typeOfValue;
        }

        public readonly LTSQLToken FunctionName;
        public readonly LTSQLToken[] Parameters;

        public readonly LTSQLToken Call;

        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { this.Call };
        }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitFunctionCallToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            return new FunctionCallToken(this.Call.Visit(visitor), this.ValueType);
        }
    }
}
