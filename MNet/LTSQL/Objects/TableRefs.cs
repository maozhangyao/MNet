using MNet.LTSQL.SqlTokenExtends;
using MNet.LTSQL.SqlTokens;
using System.Collections;
using System.Runtime.InteropServices;

namespace MNet.LTSQL.Objects
{
    public class TableRefs
    {
        //key:参数名
        private Dictionary<string, TableDescriptor> _refs
            = new Dictionary<string, TableDescriptor>();

        public bool Is()
        {
            return false;
        }
        public TableDescriptor GetTableRef(string parameterName)
        {
            return this._refs.TryGetValue(parameterName, out var v) ? v : null;
        }
        public void AddTableRef(string parameterName, TableDescriptor value)
        {
            if (string.IsNullOrEmpty(parameterName))
                throw new ArgumentNullException(nameof(parameterName));
            if (this._refs.ContainsKey(parameterName))
                throw new Exception("重复添加表格信息");

            this._refs.Add(parameterName, value);
        }
        public TableDescriptor BuildNewMeger(string alias)
        {
            TableDescriptor newTable = new TableDescriptor(null, alias);
            foreach (var kvp in this._refs)
            {
                TableDescriptor tableDescriptor = kvp.Value;
                if (tableDescriptor.IsHide)
                    continue;

                string tableAlias = tableDescriptor.Alias;
                foreach (FieldDescriptor field in tableDescriptor.Fields)
                {
                    newTable.AddField(new FieldDescriptor(field.Field,
                        LTSQLTokenFactory.CreateAccessToken(
                                LTSQLTokenFactory.CreateTableObjectToken(tableAlias, tableDescriptor.MappingType),
                                field.Field,
                                field.FieldValueType
                            )
                        , field.FieldValueType));
                }
            }
            return newTable;
        }
    }

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
        public TableDescriptor(string tableName) : this(tableName, null)
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
            this.IsHide = isHide;
            this.TableName = tableName;
        }
        public TableDescriptor(TableDescriptor other)
        {
            this.TableName = other.TableName;
            this.Alias = other.Alias;
            this.IsHide = other.IsHide;
            this.MappingType = other.MappingType;
            foreach (FieldDescriptor field in other.Fields)
            {
                this._fields.Add(new FieldDescriptor(field.Field, field.Value, field.FieldValueType));
            }
        }


        private List<FieldDescriptor> _fields = new List<FieldDescriptor>();


        public bool IsHide { get; }
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
