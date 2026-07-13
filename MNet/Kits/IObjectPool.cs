using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MNet.Kits
{
    /// <summary>
    /// 全局公用对象池(承诺线程安全)
    /// </summary>
    public interface IObjectPool : IDisposable
    {
        /// <summary>
        /// 当前池中，存在的所有对象数量，包括正在使用的
        /// </summary>
        int Count { get; }


        /// <summary>
        /// 取出一个对象
        /// </summary>
        /// <param name="waitTimeInMillisecond"></param>
        /// <returns></returns>
        object? Take();
        /// <summary>
        /// 取出一个对象
        /// </summary>
        /// <param name="waitTimeInMillisecond"></param>
        /// <returns></returns>
        object? Take(int waitTimeInMillisecond);


        /// <summary>
        /// 归还
        /// </summary>
        /// <param name="obj"></param>
        void Return(object obj);
    }
}
