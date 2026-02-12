using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 用于计算表名的过渡token
    /// </summary>
    internal class PrefixPropToken : BogusToken
    {
        public PrefixPropToken() 
        { }
        public PrefixPropToken(string prefix)
        {
            this.ObjPrefix = prefix;
        }

        public string ObjPrefix { get; set; }
    }
}
