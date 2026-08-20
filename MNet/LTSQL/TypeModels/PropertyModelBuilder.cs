using System;
using System.Reflection;

namespace MNet.LTSQL.TypeModels
{
    /// <summary>
    /// 属性构造
    /// </summary>
    public class PropertyModelBuilder 
    {
        public PropertyModelBuilder() : this(null)
        { }
        public PropertyModelBuilder(PropertyDescriptor seed)
        {
            this._seed = seed ?? new PropertyDescriptor();
        }


        private PropertyDescriptor _seed;


        public PropertyDescriptor Build()
        {
            return this._seed;
        }
        public PropertyModelBuilder WithPropName(string propName)
        {
            this._seed.PropNameOnType = propName;
            return this;
        }
        public PropertyModelBuilder WithColumns(string columnName)
        {
            this._seed.PropNameOnData = columnName;
            return this;
        }
        public PropertyModelBuilder WithMember(MemberInfo member)
        {
            this._seed.Property = member;
            return this;
        }
        public PropertyModelBuilder WithPropertyType(Type type)
        {
            this._seed.PropertyType = type;
            return this;
        }
        public PropertyModelBuilder WithPosition(int position)
        {
            this._seed.Position = position;
            return this;
        }
        public PropertyModelBuilder WithAttr(Attribute attr)
        {
            if (attr == null)
                throw new ArgumentNullException(nameof(attr));

            this._seed.Attrs.Add(attr);
            return this;
        }
        public PropertyModelBuilder WithAttrs(params Attribute[] attrs)
        {
            if (attrs == null)
                throw new ArgumentNullException(nameof(attrs));

            foreach (Attribute attr in attrs)
                this.WithAttr(attr);

            return this;
        }
    }
}
