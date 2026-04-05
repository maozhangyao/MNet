using System;
using System.Collections.Generic;
using MNet.LTSQL.v1.SqlTokens;

namespace MNet.LTSQL.v1.SqlTokenExtends
{
    /// <summary>
    /// 表示支持取反操作
    /// </summary>
    public interface INotable
    {
        LTSQLToken Not();
    }
}
