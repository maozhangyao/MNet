using MNet.LTSQL.Objects;
using MNet.LTSQL.SqlTokenExtends;
using MNet.LTSQL.TypeModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 表示 table 对象
    /// </summary>
    public class TableObjectToken : ObjectToken, ITupleable
    {
        internal TableObjectToken(string tbObjName, EntityTypeDescriptor descriptor, Type typeOfObject)
            : base(SqlObjectType.Table, tbObjName, typeOfObject)
        {
            this.Descriptor = descriptor;
        }


        public EntityTypeDescriptor Descriptor { get; }
        public Type MappingType => this.ValueType;
        public LTSQLToken this[string key]
        {
            get
            {
                PropertyDescriptor prop = this.Descriptor.Properties.FirstOrDefault(p => p.PropNameOnType == key);
                if (prop == null)
                    return null;

                return LTSQLTokenFactory.CreateFieldToken(prop.PropNameOnData, prop.PropNameOnType, prop.PropertyType);
            }
        }


        public Type GetValueType(string key)
        {
            return this.Descriptor.Properties.FirstOrDefault(p => p.PropNameOnType == key)?.PropertyType;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<(string key, LTSQLToken value)> GetEnumerator()
        {
            foreach (PropertyDescriptor prop in this.Descriptor.Properties)
                yield return (prop.PropNameOnType, LTSQLTokenFactory.CreateFieldToken(prop.PropNameOnData, prop.PropNameOnType, prop.PropertyType));
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
