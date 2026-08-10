using MNet.LTSQL.Objects;
using MNet.LTSQL.SqlTokenExtends;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 命名表格引用
    /// </summary>
    public class TableRefToken : ObjectToken, ITupleable
    {
        internal TableRefToken(string alias, TableDescriptor descritpor)
            : base(SqlObjectType.Table, descritpor.TableName, descritpor.MappingType)
        {
            this.Alias = alias;
            this.Descriptor = descritpor;
        }


        public string Alias { get; }
        public TableDescriptor Descriptor { get; }
        public Type MappingType { get => this.ValueType; }
        public LTSQLToken this[string key] { get => this.Descriptor[key]; }


        
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
            return visitor.VisitTableRefToken(this);
        }
        public override string ToString()
        {
            return $"{this.Descriptor.TableName} AS {this.Alias}";
        }
    }
}
