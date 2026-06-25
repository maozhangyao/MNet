using MNet.LTSQL.SqlTokenExtends;
using MNet.LTSQL.SqlTokens;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MNet.LTSQL.Objects
{
    public class FieldDescriptor
    {
        public FieldDescriptor()
        {　}
        public FieldDescriptor(string field, LTSQLToken token, Type valueType)
        {
            this.Field = field;
            this.Value = token;
            this.FieldValueType = valueType;
        }

        public string     Field { get; set; }
        public LTSQLToken Value { get; set; }
        public Type FieldValueType { get; set; }
    }

    public class TableDescriptor : ITupleable
    {
        public TableDescriptor()
        { }
        public TableDescriptor(string tableName, string alias) : this(tableName, alias, null)
        { }
        public TableDescriptor(string tableName, string alias, Type type)
        {
            this.Alias = alias;
            this.TableName = tableName;
            this.MappingType = type;
        }
        public TableDescriptor(string tableName, bool isHide)
        {
            this.TableName = tableName;
        }
        public TableDescriptor(TableDescriptor other)
        {
            this.TableName = other.TableName;
            this.Alias = other.Alias;
            this.MappingType = other.MappingType;
            foreach (FieldDescriptor field in other.Fields)
            {
                this._fields.Add(new FieldDescriptor(field.Field, field.Value, field.FieldValueType));
            }
        }


        private List<FieldDescriptor> _fields = new List<FieldDescriptor>();


        public string Alias { get; set; }
        public string TableName { get; set; }
        public IEnumerable<FieldDescriptor> Fields => this._fields;
        public Type MappingType { get; set; }
        public LTSQLToken this[string prop] => this._fields.FirstOrDefault(p => p.Field == prop)?.Value;


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<(string key, LTSQLToken value)> GetEnumerator()
        {
            return this.Fields.Select(p => (p.Field, p.Value)).GetEnumerator();
        }
        public void AddField(FieldDescriptor field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            this._fields.Add(field);
        }
        public FieldDescriptor GetField(string key)
        {
            return this._fields.FirstOrDefault(p => p.Field == key);
        }
        public Type GetValueType(string key)
        {
            return this._fields.FirstOrDefault(p => p.Field == key)?.FieldValueType;
        }
    }
}
