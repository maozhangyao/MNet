using System;
using System.Collections.Generic;
using MNet.LTSQL.SqlTokens;

namespace MNet.LTSQL.SqlTokenExtends
{
    /// <summary>
    /// 表示支持取反操作
    /// </summary>
    public interface INotable
    {
        bool IsNot { get; }

        LTSQLToken Not();
    }
}
