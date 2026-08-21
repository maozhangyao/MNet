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
    /// 命名表格引用
    /// </summary>
    public class TableRefToken : ObjectToken, ITupleable
    {
        internal TableRefToken(string alias, EntityTypeDescriptor descriptor)
            : base(SqlObjectType.Table, descriptor.TableName, descriptor.Type)
        {
            this.Alias = alias;
            this.Descriptor = descriptor;
        }


        public string Alias { get; }
        public EntityTypeDescriptor Descriptor { get; }
        public Type MappingType { get => this.ValueType; }
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
            return visitor.VisitTableRefToken(this);
        }
        public override string ToString()
        {
            return $"{this.Alias}";
        }
    }
}
