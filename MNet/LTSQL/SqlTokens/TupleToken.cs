using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Sources;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 一个元组概念
    /// </summary>
    public class TupleToken : ValueToken
    {
        public TupleToken(Type type)
        {
            this.ValueType = type;
        }

        private List<(LTSQLToken, string)> _props;

        /// <summary>
        /// 返回属性名
        /// </summary>
        public string[] PropNames => this._props?.Select(p => p.Item2)?.ToArray() ?? new string[0];
        /// <summary>
        /// 返回属性值
        /// </summary>
        public LTSQLToken[] Props => this._props?.Select(p => p.Item1)?.ToArray() ?? new LTSQLToken[0];
        public IEnumerable<(LTSQLToken, string)> Items => this._props ?? new List<(LTSQLToken, string)>(0);


        public LTSQLToken GetProp(string prop)
        {
            if (this._props == null)
                return null;

            return this._props.FirstOrDefault(p => p.Item2 == prop).Item1;
        }
        public void Add(LTSQLToken token, string name)
        {
            this._props ??= new List<(LTSQLToken, string)>();
            this._props.Add((token, name));
        }
    }
}
