using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 表示分组对象
    /// </summary>
    public class GroupObjToken : BogusToken
    {
        public GroupObjToken() { }
        public GroupObjToken(LTSQLToken groupElement, LTSQLToken groupKey)
        {
            this.Element = groupElement;
            this.GroupKey = groupKey;
        }

        /// <summary>
        /// 分组元素
        /// </summary>
        public LTSQLToken Element { get; set; }
        /// <summary>
        /// 分组依据
        /// </summary>
        public LTSQLToken GroupKey { get; set; }
    }
}
