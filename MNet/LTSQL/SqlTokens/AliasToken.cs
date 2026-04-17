using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 别名token: obj as name1
    /// </summary>
    public class AliasToken : LTSQLToken
    {
        internal AliasToken(LTSQLToken obj, string alias)
        {
            this.Object = obj;
            this.Alias = alias;
        }

        public readonly string Alias;
        public readonly LTSQLToken Object;
        

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitAliasToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            return new AliasToken(this.Object.Visit(visitor), this.Alias);
        }
    }
}
