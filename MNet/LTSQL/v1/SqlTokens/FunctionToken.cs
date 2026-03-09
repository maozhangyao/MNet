using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class FunctionToken : SqlValueToken
    {
        public FunctionToken()
        { }
        public FunctionToken(string funcName)
        {
            this.FunctionName = funcName;
        }
        public FunctionToken(string funcName, LTSQLToken[] parameters)
        {
            this.FunctionName = funcName;
            this.Parameters = parameters;
        }
        public FunctionToken(string funcName, LTSQLToken[] parameters, Type typeOfValue)
        {
            this.FunctionName = funcName;
            this.Parameters = parameters;
            this.ValueType = typeOfValue;
        }

        public string FunctionName { get; set; }
        public LTSQLToken[] Parameters { get; set; }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return this.Parameters;
        }

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitFunctionToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            if(this.Parameters != null)
            {
                for(int i = 0; i < this.Parameters.Length; i++)
                {
                    LTSQLToken param = this.Parameters[i];
                    this.Parameters[i] = param.Visit(visitor);
                    
                }
            }
            return this;
        }
    }
}
