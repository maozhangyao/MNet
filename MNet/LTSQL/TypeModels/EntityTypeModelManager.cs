using MNet.LTSQL.Attributes;
using MNet.LTSQL.Objects;
using MNet.LTSQL.SqlTokens;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
#if NET6_0_OR_GREATER
using System.ComponentModel.DataAnnotations.Schema;
#endif

namespace MNet.LTSQL.TypeModels
{
    /// <summary>
    /// 默认的实体类型模型构造器
    /// </summary>
    public class EntityTypeModelManager : IEntityTypeModelBuilder
    {
        public EntityTypeModelManager() : this(null)
        {

        }
        public EntityTypeModelManager(EntityTypeModelOptions options)
        {
            this._options = options;
        }


        private EntityTypeModelOptions _options;


        //如果tableName是 `{schema}.{table}` 格式，则拆分返回 table 和 schema
        private void SplitTableSchema(string tableName, out string table, out string schema)
        {
            table = schema = null;
            if (tableName == null)
                return;

            int pos = tableName.TrimStart('.').IndexOf('.');
            table = tableName;
            if (pos > 0)
            {
                schema = tableName.Substring(0, pos);
                table = tableName.Substring(pos + 1);
            }
        }

        public EntityTypeDescriptor Build(Type entityType, EntityTypeModelBuildOptions buildOptions)
        {
            EntityTypeModelBuilder builder = new EntityTypeModelBuilder();

            string table = buildOptions.TableName;
            string schema = buildOptions.Schema;
            if (string.IsNullOrEmpty(table) && this._options?.GetTableName != null)
            {
                //解析 {schema}.{table} 格式
                string tableName = this._options.GetTableName(entityType, buildOptions);
                if (!string.IsNullOrEmpty(schema))
                    SplitTableSchema(tableName, out table, out _); //schema不为空，则不覆盖
                else
                    SplitTableSchema(tableName, out table, out schema);
            }

            if (string.IsNullOrEmpty(table))
            {
#if NET6_0_OR_GREATER
                TableAttribute attr1 = entityType.GetCustomAttribute<TableAttribute>();
                QTableAttribute attr2 = entityType.GetCustomAttribute<QTableAttribute>();
                table = attr1?.Name ?? attr2?.Name ?? entityType.Name;
                if (string.IsNullOrEmpty(schema))
                    schema = attr1?.Schema ?? attr2?.Schema;
#else
                QTableAttribute attr = entityType.GetCustomAttribute<QTableAttribute>();
                table = attr?.Name ?? entityType.Name;
                if (string.IsNullOrEmpty(schema))
                    schema = attr?.Schema;
#endif
            }

            if (buildOptions.Refer is IEntityTypeModel model)
            {
                buildOptions.Schema = schema;
                buildOptions.TableName = table;
                model.Build(builder, buildOptions);
                return builder.Build();
            }


            builder.WithTableName(table)
                .WithSchema(schema)
                .WithRefer(buildOptions.Refer)
                .WithType(entityType)
                .WithAttrs(Attribute.GetCustomAttributes(entityType, true));

            //解析属性
            int i = 0;
            foreach (PropertyInfo prop in entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (prop.IsDefined(typeof(NonFiledAttribute)))
                    continue;

                string column = this._options.GetColumnName(entityType, prop, buildOptions);
                builder.WithProperty(prop.Name)
                    .WithPropertyType(prop.PropertyType)
                    .WithColumns(column)
                    .WithPosition(i++)
                    .WithMember(prop)
                    .WithAttrs(Attribute.GetCustomAttributes(prop, false));
            }

            //解析字段
            i = 0;
            foreach (FieldInfo prop in entityType.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (prop.IsDefined(typeof(NonFiledAttribute)))
                    continue;

                string column = this._options.GetColumnName(entityType, prop, buildOptions);
                builder.WithProperty(prop.Name)
                    .WithPropertyType(prop.FieldType)
                    .WithColumns(column)
                    .WithPosition(i++)
                    .WithMember(prop)
                    .WithAttrs(Attribute.GetCustomAttributes(prop, false));
            }

            return builder.Build();
        }

        /// <summary>
        /// 返回指定类型的实体模型
        /// </summary>
        /// <param name="t"></param>
        /// <param name="options"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static EntityTypeDescriptor GetEntityTypeModel(Type t, EntityTypeModelOptions options, Action<EntityTypeModelBuildOptions> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            EntityTypeModelBuildOptions typeModelBuildOptions = new EntityTypeModelBuildOptions();
            configure(typeModelBuildOptions);

            //外部生成
            if (options.EntityTypeModelBuilderFactory != null)
            {
                IEntityTypeModelBuilder entityModelTypeBuilder = options.EntityTypeModelBuilderFactory();
                return entityModelTypeBuilder.Build(t, typeModelBuildOptions);
            }

            if (options.EntityTypeModelFactory != null)
            {
                EntityTypeModelBuilder builder = new EntityTypeModelBuilder();
                options.EntityTypeModelFactory(builder, typeModelBuildOptions, t);
                return builder.Build();
            }

            //默认的内部生成
            return new EntityTypeModelManager(options)
                .Build(t, typeModelBuildOptions);
        }
    }
}
