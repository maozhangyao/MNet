using System.Linq;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 表示一组值列表，如：(1,2,3,4,5,6)
    /// </summary>
    public class ValuesListToken : SequenceToken
    {
        public ValuesListToken(IEnumerable<LTSQLToken> values) : base(values?.ToArray())
        { }
    }
}