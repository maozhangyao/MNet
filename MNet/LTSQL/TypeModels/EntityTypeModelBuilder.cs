using System;

namespace MNet.LTSQL.TypeModels
{
    /// <summary>
    /// 实体类型构造
    /// </summary>
    public class EntityTypeModelBuilder
    {
        public EntityTypeModelBuilder()
        {
            this._type = new EntityTypeDescriptor();
        }

        
        private EntityTypeDescriptor _type;


        public EntityTypeModelBuilder WithRefer(object refer)
        {
            this._type.Refer = refer;
            return this;
        }
        public EntityTypeModelBuilder WithType(Type type)
        {
            this._type.Type = type;
            return this;
        }
        public EntityTypeModelBuilder WithSchema(string schema)
        {
            this._type.TableSchema = schema;
            return this;
        }
        public EntityTypeModelBuilder WithTableName(string table)
        {
            this._type.TableName = table;
            return this;
        }
        public EntityTypeModelBuilder WithAttr(Attribute attr)
        {
            if (attr == null)
                throw new ArgumentNullException(nameof(attr));

            this._type.Attrs.Add(attr);
            return this;
        }
        public EntityTypeModelBuilder WithAttrs(params Attribute[] attrs)
        {
            if (attrs == null)
                throw new ArgumentNullException(nameof(attrs));

            foreach (Attribute attr in attrs)
                this.WithAttr(attr);

            return this;
        }
        public PropertyModelBuilder WithProperty(string propName)
        {
            PropertyDescriptor descriptor = new PropertyDescriptor();
            descriptor.PropNameOnType = propName;
            this.WithProperty(descriptor);

            PropertyModelBuilder builder = new PropertyModelBuilder(descriptor);
            return builder;
        }
        public EntityTypeModelBuilder WithProperty(PropertyDescriptor prop)
        {
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));

            this._type.AddProperty(prop);
            return this;
        }
        public EntityTypeModelBuilder WithProperty(Action<PropertyModelBuilder> propertyBuilder)
        {
            if (propertyBuilder == null)
                throw new ArgumentNullException(nameof(propertyBuilder));

            PropertyModelBuilder builder = new PropertyModelBuilder();
            propertyBuilder(builder);
            this._type.AddProperty(builder.Build());

            return this;
        }
        public EntityTypeDescriptor Build()
        {
            return this._type;
        }
    }
}
