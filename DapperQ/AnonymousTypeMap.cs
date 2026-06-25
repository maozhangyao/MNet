using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DapperQ
{
    internal class AnonymousTypeMap : SqlMapper.ITypeMap
    {
        private class DefMemberMap : SqlMapper.IMemberMap
        {
            public string ColumnName { get; set; }

            public Type MemberType { get; set; }

            public PropertyInfo? Property { get; set; }

            public FieldInfo? Field { get; set; }

            public ParameterInfo? Parameter { get; set; }
        }


        public AnonymousTypeMap(Type anonymouseType)
        {
            this._type = anonymouseType;
        }


        private Type _type;

        ConstructorInfo? SqlMapper.ITypeMap.FindConstructor(string[] names, Type[] types)
        {
            ConstructorInfo ctor = this._type.GetConstructors()[0];
            string[] args = ctor.GetParameters().Select(p => p.Name).ToArray()!;
            if (args.Length != names.Length)
                return null;
            if (!args.All(p => names.Contains(p)))
                return null;

            return ctor;
        }

        ConstructorInfo? SqlMapper.ITypeMap.FindExplicitConstructor()
        {
            return null;//匿名类型没有无参构造
        }

        SqlMapper.IMemberMap? SqlMapper.ITypeMap.GetConstructorParameter(ConstructorInfo constructor, string columnName)
        {
            DefMemberMap def = new DefMemberMap();
            def.Field = this._type.GetField(columnName);
            def.Property = this._type.GetProperty(columnName);
            def.Parameter = constructor.GetParameters().First(p => p.Name == columnName);
            def.ColumnName = columnName;
            def.MemberType = def.Field?.FieldType ?? def.Property?.PropertyType;

            return def;
        }

        SqlMapper.IMemberMap? SqlMapper.ITypeMap.GetMember(string columnName)
        {
            DefMemberMap def = new DefMemberMap();
            def.Field = this._type.GetField(columnName);
            def.Property = this._type.GetProperty(columnName);
            def.Parameter = null;
            def.ColumnName = columnName;
            def.MemberType = def.Field?.FieldType ?? def.Property?.PropertyType;
            return def;
        }
    }
}
