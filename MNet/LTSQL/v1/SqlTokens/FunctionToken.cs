using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class FunctionToken : LTSQLToken
    {
        public AliasToken FunctionName { get; set; }
        public List<LTSQLToken> Parameters { get; set; }
        public override string ToSql()
        {
            return "Function";
        }
    }

    
}
