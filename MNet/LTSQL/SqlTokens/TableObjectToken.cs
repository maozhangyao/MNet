using MNet.LTSQL.Objects;
using MNet.LTSQL.SqlTokenExtends;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
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
            return $"({this.Descriptor.TableName})" + this.ObjectName + $":{ObjectType}";
        }
    }
}
