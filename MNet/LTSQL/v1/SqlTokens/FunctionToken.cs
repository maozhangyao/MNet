using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class FunctionToken : SQLValueToken
    {
        public FunctionToken()
        { }
        public FunctionToken(string funcName)
        {
            this.FunctionName = funcName;
        }

        public string FunctionName { get; set; }
        public List<LTSQLToken> Parameters { get; set; }
        public override string ToSql()
        {
            return "Function";
        }
    }
}
