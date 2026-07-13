using MNet.Kits;
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
        /// <summary>
        /// 数据库连接器
        /// </summary>
        IObjectRenting<IDbConnection> ConnectionRenting { get; }

        /// <summary>
        /// 创建查询表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ILTSQLObjectQueryable<T> CreateQuery<T>();
        /// <summary>
        /// 创建非查询表达式:Update
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ILTSQLNonQueryable<T> CreateUpdate<T>();
        /// <summary>
        /// 创建非查询表达式:delete
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ILTSQLNonQueryable<T> CreateDelete<T>();
        /// <summary>
        /// 将当前上下文附加到表达式
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        ILTSQLQueryable Follow(ILTSQLQueryable expr);
    }
}