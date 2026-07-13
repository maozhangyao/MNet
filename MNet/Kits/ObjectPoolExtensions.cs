using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.Kits
{
    public static class ObjectPoolExtensions
    {
        /// <summary>
        /// 租用对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pool"></param>
        /// <returns></returns>
        public static IObjectRenting<T> Rent<T>(this IObjectPool pool)
        {
            return new ObjectPoolRenting<T>(pool);
        }
    }
}
