using MNet.LTSQL.Objects;
using MNet.LTSQL.SqlTokenExtends;
using System;
using System.Collections;
using System.Collections.Generic;

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
            this.Alias = objectName;
            this.ObjectType = objectType;
            this.ValueType = typeOfObject;
        }

        //对象名
        public readonly string Alias;
        public readonly SqlObjectType ObjectType;


        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitObjectToken(this);
        }
        public override string ToString()
        {
            return this.Alias + $":{ObjectType}";
        }
    }


    /// <summary>
    /// 表示 table 对象
    /// </summary>
    public class TableObjectToken : ObjectToken, ITupleable
    {
        internal TableObjectToken(string tbObjName, TableDescriptor descriptor, Type typeOfObject)
            : base(SqlObjectType.Table, tbObjName, typeOfObject)
        {
            this.Descriptor = descriptor;
        }


        public TableDescriptor Descriptor { get; }
        public Type MappingType => this.ValueType;
        public LTSQLToken this[string key] => this.Descriptor[key];


        public Type GetValueType(string key)
        {
            return this.Descriptor.GetValueType(key);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<(string key, LTSQLToken value)> GetEnumerator()
        {
            return this.Descriptor.GetEnumerator();
        }
        
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitTableObjectToken(this);
        }
        public override string ToString()
        {
            return $"({this.Descriptor.TableName})" + this.Alias + $":{ObjectType}";
        }
    }
}
