using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Sources;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 一个元组概念
    /// </summary>
    public class TupleToken : BogusToken
    {
        public TupleToken() { }

        private List<(LTSQLToken, string)> _props;

        /// <summary>
        /// tuple 表示的类型
        /// </summary>
        public Type Type { get; set; }
        public LTSQLToken[] Props => this._props?.Select(p => p.Item1)?.ToArray() ?? new LTSQLToken[0];
        public IEnumerable<(LTSQLToken, string)> Items => this._props ?? new List<(LTSQLToken, string)>(0);


        public void Add(LTSQLToken token, string name)
        {
            this._props ??= new List<(LTSQLToken, string)>();
            this._props.Add((token, name));
        }
    }
}
