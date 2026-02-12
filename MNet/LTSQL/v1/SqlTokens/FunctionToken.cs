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


        public override void ToSql(LTSQLTokenContext context)
        {
            context.SQLBuilder.Append(this.FunctionName);
            context.SQLBuilder.Append('(');

            bool comma = false;
            foreach(var param in Parameters)
            {
                if (comma)
                    context.SQLBuilder.Append(',');
                comma = true;
                param.ToSql(context);
            }

            context.SQLBuilder.Append(')');
        }
    }
}
