using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 一个sql对象，如：表名称
    /// 注意其在翻译过程中，需要关键字转义，所以不是单纯的文本
    /// </summary>
    public class AliasToken : ValueToken
    {
        public AliasToken()
        { }
        public AliasToken(string alias)
        {
            this.Alias = alias;
        }

        public string Alias { get; set; }

        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return Array.Empty<LTSQLToken>();
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitAliasToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            return this;
        }
    }
}
