using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MNet.Kits
{
    /// <summary>
    /// 对象租用(非线程安全)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObjectRenting<T> : IDisposable
    {
        /// <summary>
        /// 租用的对象
        /// </summary>
        /// <returns></returns>
        T Object { get; }
    }

    public class ObjectPoolRenting<T> : IObjectRenting<T>
    {
        public ObjectPoolRenting(IObjectPool pool)
        {
            this._pool = pool;
            this._obj = null;
            this._rented = false;
            this._disposed = false;
        }

        private object _obj;
        private bool _rented;
        private bool _disposed;
        private IObjectPool _pool;


        public T Object
        {
            get
            {
                if (this._disposed)
                    throw new ObjectDisposedException(nameof(ObjectPoolRenting<T>));

                if (this._rented)
                    return (T)this._obj;

                this._rented = true;
                this._obj = this._pool.Take();
                return (T)this._obj;
            }
        }

        public void Dispose()
        {
            if (this._disposed)
                return;

            if (this._rented)
                this._pool.Return(this._obj);

            this._rented = false;
            this._disposed = true;
        }
        ~ObjectPoolRenting()
        {
            this.Dispose();
        }
    }
}
