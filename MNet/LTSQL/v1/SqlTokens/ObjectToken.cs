using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 一个sql对象，如：表对象，函数对象
    /// 注意其在翻译过程中，需要关键字转义，所以不是单纯的文本
    /// </summary>
    public class ObjectToken : ValueToken
    {
        internal ObjectToken(SqlObjectType objectType, string objectName, Type typeOfObject)
        {
            this.Alias = objectName;
            this.ObjectType = objectType;
            this.ValueType = typeOfObject;
        }


        public readonly string Alias;
        public readonly SqlObjectType ObjectType;


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new LTSQLToken[0];
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitObjectToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            return this;
        }
    }
}
