using MNet.LTSQL.SqlTokenExtends;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Sources;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 一个元组概念
    /// </summary>
    public class TupleToken : ValueToken, ITupleable
    {
        internal TupleToken(Type type)
        {
            this.ValueType = type;
        }

        private List<(string, LTSQLToken, Type)> _props;


        public Type MappingType => this.ValueType;
        /// <summary>
        /// 返回属性名
        /// </summary>
        public string[] PropNames => this._props?.Select(p => p.Item1)?.ToArray() ?? new string[0];
        /// <summary>
        /// 返回属性值
        /// </summary>
        public LTSQLToken[] Props => this._props?.Select(p => p.Item2)?.ToArray() ?? new LTSQLToken[0];
        public LTSQLToken this[string key]
        {
            get
            {
                if (this._props == null)
                    return null;
                if (this._props.Any(p => p.Item1 == key))
                    return this._props.First(p => p.Item1 == key).Item2;
                return null;
            }
        }


        public Type GetValueType(string key)
        {
            if (this._props == null)
                return null;
            if (this._props.Any(p => p.Item1 == key))
                return this._props.First(p => p.Item1 == key).Item3;
            return null;
        }
        public LTSQLToken GetValue(string prop)
        {
            return this[prop];
        }
        public void Add(string name, LTSQLToken value, Type valueType)
        {
            this._props ??= new List<(string, LTSQLToken, Type)>();
            this._props.Add((name, value, valueType));
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<(string key, LTSQLToken value)> GetEnumerator()
        {
            if(this._props == null)
                return (Array.Empty<(string, LTSQLToken)>() as IEnumerable<(string, LTSQLToken)>).GetEnumerator();
            return this._props.Select(p => (p.Item1, p.Item2)).GetEnumerator();
        }
    }
}
