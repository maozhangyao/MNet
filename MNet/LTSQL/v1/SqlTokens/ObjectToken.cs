using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 一个sql对象，如：表名称
    /// 注意其在翻译过程中，需要关键字转义，所以不是单纯的文本
    /// </summary>
    public class ObjectToken : ValueToken
    {
        public ObjectToken()
        { }
        public ObjectToken(LTSQLToken alias, Type objectType)
        {
            this.Alias = alias;
        }

        public readonly LTSQLToken Alias;

        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { this.Alias };
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitObjectToken(this);
        }

        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            return new ObjectToken(this.Alias.Visit(visitor), this.ValueType);
        }
    }
}
