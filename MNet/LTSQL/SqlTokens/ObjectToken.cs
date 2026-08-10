using MNet.LTSQL.Objects;
using MNet.LTSQL.SqlTokenExtends;
using System;
using System.Collections;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 一个sql对象，如：表对象，函数对象
    /// 注意其在翻译过程中，需要关键字转义，所以不是单纯的文本
    /// </summary>
    public class ObjectToken : ValueToken
    {
        internal ObjectToken(SqlObjectType objectType, string objectName, Type typeOfObject)
        {
            this.ObjectName = objectName;
            this.ObjectType = objectType;
            this.ValueType = typeOfObject;
        }

        //对象名
        public readonly string ObjectName;
        public readonly SqlObjectType ObjectType;


        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitObjectToken(this);
        }
        public override string ToString()
        {
            return this.ObjectName + $":{ObjectType}";
        }
    }
}
