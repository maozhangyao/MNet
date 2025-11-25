using System;

namespace MNet.SqlExpression
{
    /// <summary>
    /// 一个唯一命名生成器
    /// </summary>
    public class NamedCreator
    {
        public NamedCreator() : this(0, null)
        { }
        public NamedCreator(Func<int, string> generator) : this(0, generator)
        { }
        public NamedCreator(int seed, Func<int, string> generator)
        {
            this._seed = seed;
            this._fmt = generator ?? (i => $"_{i}");
            this.Next();
        }


        private int _seed;
        private Func<int, string> _fmt;

        public string Current { get; private set; }

        public string Next()
        {
            this.Current = this._fmt(this._seed);
            this._seed++;
            return this.Current;
        }
    }
}
