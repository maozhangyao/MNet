using MNet.LTSQL.SqlTokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNet.LTSQL.SqlTokenExtends
{
    public interface ITupleable : IEnumerable<(string key, LTSQLToken value)>
    {
        Type MappingType { get; }
        LTSQLToken this[string key] { get; }
        Type GetValueType(string key);
    }
}
