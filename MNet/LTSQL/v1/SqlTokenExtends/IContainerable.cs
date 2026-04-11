using MNet.LTSQL.v1.SqlTokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.v1.SqlTokenExtends
{
    /// <summary>
    /// 仅仅表示一个容器, 按照特定顺序存储token集合， 没有任何语义存在
    /// </summary>
    public interface IContainerable : IEnumerable<LTSQLToken>
    {
    }
}
