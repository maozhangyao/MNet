using MNet.LTSQL.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 用于计算表名的过渡token
    /// </summary>
    internal class PrefixPropToken : BogusToken
    {
        internal PrefixPropToken(string prefix)
        {
            this.ObjPrefix = prefix;
        }
        internal PrefixPropToken(string prefix, TableRefs refs)
        {
            this.TableRefs = refs;
            this.ObjPrefix = prefix;
        }


        public string ObjPrefix { get; set; }

        public TableRefs TableRefs { get; set; }
    }
}
