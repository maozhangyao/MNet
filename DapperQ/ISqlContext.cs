using MNet.LTSQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperQ
{
    /// <summary>
    /// 具有相同连接，相同配置的sql上下文
    /// </summary>
    public interface ISqlContext : IDisposable
    {
        IDbConnection Connection { get; }

        ILTSQLObjectQueryable<T> Create<T>() where T : class, new();
    }
}