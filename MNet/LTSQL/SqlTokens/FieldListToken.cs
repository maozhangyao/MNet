using System;
using System.Linq;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 一组字段列表，如：select,order,group等等中的字段列表
    /// </summary>
    public class FieldListToken : SequenceToken
    {
        public FieldListToken(IEnumerable<LTSQLToken> fields) : base(fields?.ToArray())
        { }
    }
}