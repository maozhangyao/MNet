using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MNet.Kits
{
    /// <summary>
    /// 通用对象池
    /// </summary>
    public class GeneralObjectPool : IObjectPool
    {
        protected GeneralObjectPool(int minSize)
        {
            if (minSize < 0)
                throw new Exception("最小对象数量必须大于等于0");

            this._min = minSize;
            this._backClearDisp = this.UseBackClear();
        }
        public GeneralObjectPool(Func<object> constructor, int minSize) : this(minSize)
        {
            if (constructor == null)
                throw new ArgumentNullException(nameof(constructor));

            this._ctor = constructor;
        }


        private class PoolItem
        {
            public PoolItem(object val)
            {
                this._value = val;
                this._time = DateTime.Now;
            }
            public object _value;
            public DateTime _time;
        }

        private int _count;
        private bool _disposed;
        private readonly int _min;
        private readonly object _lock = new object();
        private readonly Action _backClearDisp;

        private Func<object> _ctor;
        private ConcurrentBag<PoolItem> _available = new ConcurrentBag<PoolItem>();
        private ConcurrentDictionary<long, PoolItem> _actived = new ConcurrentDictionary<long, PoolItem>();


        public int Count => this._count;


        protected virtual object? ConstructObject()
        {
            return this._ctor == null ? null : this._ctor();
        }


        private void ThrowIfDisposed()
        {
            if ((this._disposed))
                throw new ObjectDisposedException(this.GetType().FullName);

            lock (_lock)
            {
                if ((this._disposed))
                    throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
        private void IncrCount()
        {
            Interlocked.Increment(ref this._count);
        }
        private void ReleaseCount()
        {
            Interlocked.Decrement(ref this._count);
        }
        private Action UseBackClear()
        {
            CancellationTokenSource src = new CancellationTokenSource();
            CancellationToken token = src.Token;
            Task.Factory.StartNew(async () => {

                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(45 * 1000);
                    PoolItem item;
                    while (!token.IsCancellationRequested && this._available.Count > this._min && this._available.TryTake(out item))
                    {
                        if (item._value is IDisposable disp)
                        {
                            try
                            {
                                disp.Dispose();
                            }
                            catch (Exception ex)
                            { }
                        }
                        this.ReleaseCount();
                    }
                    //this.ThrowIfDisposed();
                }
#if DEBUG
                Console.WriteLine($"对象池后台线程已退出");
#endif
            });

            return () =>
            {
                src.Cancel();
                src.Dispose();
            };
        }


        public void Dispose()
        {
            if (_disposed)
                return;

            lock (_lock)
            {
                if (_disposed)
                    return;

                this._disposed = true;
                if (this._backClearDisp != null)
                    this._backClearDisp();
            }


            foreach (var item in this._available)
            {
                if (item._value is IDisposable disp)
                    disp.Dispose();
            }

            foreach ((_, var item) in this._actived)
            {
                if (item._value is IDisposable disp)
                    disp.Dispose();
            }

            this._ctor = null;
            this._actived = null;
        }
        public void Return(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            lock (_lock)
            {
                this.ThrowIfDisposed();
                int hc = obj.GetHashCode();
                if (!_actived.TryRemove(hc, out PoolItem item) || item._value != obj)
                    throw new Exception("实例非当前池中创建或者池中没有活跃的对象");

                this._available.Add(item);
            }
        }


        public object? Take()
        {
            lock (_lock)
            {
                this.ThrowIfDisposed();

                if (this._available.TryTake(out var item))
                {
                    this._actived.TryAdd(item._value.GetHashCode(), item);
                    return item._value;
                }

                try
                {
                    object val = null;
                    lock (this._lock)
                        val = this.ConstructObject();

                    if (val == null)
                        return val;

                    int hc = val.GetHashCode();
                    PoolItem item1 = new PoolItem(val);
                    this._actived.TryAdd(hc, item1);
                    this.IncrCount();
                    return val;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        public object? Take(int waitTimeInMillisecond)
        {
            return this.Take();
        }
        ~GeneralObjectPool()
        {
            this.Dispose();
        }
    }
}