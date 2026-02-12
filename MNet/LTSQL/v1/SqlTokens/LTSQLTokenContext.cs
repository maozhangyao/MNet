using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class LTSQLTokenContext
    {
        public LTSQLOptions Options { get; set; }
        public StringBuilder SQLBuilder { get; set; }
    }
}
