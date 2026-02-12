using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 能够求出一个值
    /// </summary>
    public abstract class ValueToken : LTSQLToken
    {
        /// <summary>
        /// 值对应的类型
        /// </summary>
        public Type ValueType { get; set; }
    }
}
