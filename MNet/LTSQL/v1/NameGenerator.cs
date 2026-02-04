using System;

namespace MNet.LTSQL.v1
{
    //名称生成器，用于生成唯一的表名或参数名
    public class NameGenerator
    {
        public NameGenerator(Func<int, string> gen) : this(gen, 0)
        { }

        public NameGenerator(Func<int, string> gen, int seed)
        {
            this._seed = seed;
            this._generator = gen;
        }

        private int _seed;
        private Func<int, string> _generator;

        public string Next()
        {
            return this._generator(_seed++);
        }
    }
}
