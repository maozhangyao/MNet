using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using MNet.LTSQL.SqlTokenExtends;
using System.Collections;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 表示分组对象
    /// </summary>
    public class GroupObjToken : BogusToken, ITupleable
    {
        internal GroupObjToken(LTSQLToken groupElement, LTSQLToken groupKey)
        {
            this.Element = groupElement;
            this.GroupKey = groupKey;
            this.ValueType = typeof(IGrouping<,>);
        }


        /// <summary>
        /// 分组元素
        /// </summary>
        public LTSQLToken Element { get; }
        /// <summary>
        /// 分组依据
        /// </summary>
        public LTSQLToken GroupKey { get; }

        public Type MappingType => this.ValueType;

        public LTSQLToken this[string key] => key == "Key" ? this.GroupKey : null;



        public Type GetValueType(string key)
        {
            return (this[key] as ValueToken)?.ValueType;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<(string key, LTSQLToken value)> GetEnumerator()
        {
            return ((IEnumerable<(string key, LTSQLToken value)>)(new[] { ("Key", this["Key"]) })).GetEnumerator();
        }
        public override string ToString()
        {
            return $"(Key:{this.GroupKey.ToString()})";
        }
    }
}
