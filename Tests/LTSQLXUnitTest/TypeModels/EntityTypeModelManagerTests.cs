using MNet.LTSQL.Attributes;
using MNet.LTSQL.TypeModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace LTSQLXUnitTest.TypeModels
{
    /// <summary>
    /// EntityTypeModelManager 单元测试
    /// </summary>
    public class EntityTypeModelManagerTests
    {
        //---------------------------- 测试实体 ----------------------------

        [Table("t_person", Schema = "dbo")]
        private class PersonEntity
        {
            [Column("id")]
            public int Id { get; set; }

            [Column("name")]
            public string? Name { get; set; }

            public int Age { get; set; }

            public string? Remark = null;
        }

        [QTable("q_course", Schema = "qc")]
        private class CourseEntity
        {
            [QColumn("cid")]
            public int Id { get; set; }

            [QColumn("cname")]
            public string? Name { get; set; }
        }

        private class PlainEntity
        {
            public int Id { get; set; }
        }

        private class SkipEntity
        {
            [NonFiled]
            public int SkipProp { get; set; }

            [NonFiled]
            public int SkipField = 0;

            public int Keep { get; set; }
        }

        private class CustomTableEntity
        {
            public int Id { get; set; }
        }

        private class CustomModel : IEntityTypeModel
        {
            public void Build(EntityTypeModelBuilder builder, EntityTypeModelBuildOptions options)
            {
                builder.WithTableName("from_model").WithType(typeof(PlainEntity));
                builder.WithProperty("Id")
                    .WithPropName("Id")
                    .WithColumns("id");
            }
        }

        private class CustomBuilder : IEntityTypeModelBuilder
        {
            public EntityTypeDescriptor Build(Type entityType, EntityTypeModelBuildOptions options)
            {
                return new EntityTypeModelBuilder()
                    .WithTableName("custom_builder")
                    .WithType(entityType)
                    .Build();
            }
        }

        //---------------------------- 辅助方法 ----------------------------

        /// <summary>
        /// 构造默认选项。withGetTableName=false 时不注册 GetTableName，走特性解析路径
        /// </summary>
        private static EntityTypeModelOptions CreateOptions(bool withGetTableName = true)
        {
            EntityTypeModelOptions options = new EntityTypeModelOptions
            {
                GetColumnName = (type, member, opts) =>
                {
                    ColumnAttribute? col = member.GetCustomAttribute<ColumnAttribute>();
                    QColumnAttribute? qcol = member.GetCustomAttribute<QColumnAttribute>();
                    return col?.Name ?? qcol?.Name ?? member.Name;
                }
            };

            if (withGetTableName)
            {
                options.GetTableName = (type, opts) =>
                {
                    TableAttribute? table = type.GetCustomAttribute<TableAttribute>();
                    QTableAttribute? qtable = type.GetCustomAttribute<QTableAttribute>();
                    return table?.Name ?? qtable?.Name ?? type.Name;
                };
            }

            return options;
        }

        //---------------------------- 测试用例 ----------------------------

        /// <summary>
        /// 通过 [Table]/[Column] 特性生成表名、schema 和属性列映射
        /// </summary>
        [Fact]
        public void Build_FromTableAttribute_GeneratesTableAndColumns()
        {
            EntityTypeDescriptor desc = EntityTypeModelManager.GetEntityTypeModel(
                typeof(PersonEntity), CreateOptions(withGetTableName: false), _ => { });

            Assert.Equal("t_person", desc.TableName);
            Assert.Equal("dbo", desc.TableSchema);
            Assert.Equal(typeof(PersonEntity), desc.Type);

            Assert.Equal(4, desc.Properties.Count);

            PropertyDescriptor id = Assert.Single(desc.Properties, p => p.PropNameOnType == "Id");
            Assert.Equal("id", id.PropNameOnData);
            Assert.Equal(typeof(int), id.PropertyType);
            Assert.Equal(0, id.Position);
            Assert.IsAssignableFrom<PropertyInfo>(id.Property);

            PropertyDescriptor name = Assert.Single(desc.Properties, p => p.PropNameOnType == "Name");
            Assert.Equal("name", name.PropNameOnData);
            Assert.Equal(typeof(string), name.PropertyType);

            PropertyDescriptor age = Assert.Single(desc.Properties, p => p.PropNameOnType == "Age");
            Assert.Equal("Age", age.PropNameOnData);

            PropertyDescriptor remark = Assert.Single(desc.Properties, p => p.PropNameOnType == "Remark");
            Assert.Equal("Remark", remark.PropNameOnData);
            Assert.IsAssignableFrom<FieldInfo>(remark.Property);
        }

        /// <summary>
        /// 通过 [QTable]/[QColumn] 自定义特性生成表名、schema 和列映射
        /// </summary>
        [Fact]
        public void Build_FromQTableAndQColumnAttributes()
        {
            EntityTypeDescriptor desc = EntityTypeModelManager.GetEntityTypeModel(
                typeof(CourseEntity), CreateOptions(withGetTableName: false), _ => { });

            Assert.Equal("q_course", desc.TableName);
            Assert.Equal("qc", desc.TableSchema);

            Assert.Equal(2, desc.Properties.Count);
            PropertyDescriptor id = Assert.Single(desc.Properties, p => p.PropNameOnType == "Id");
            Assert.Equal("cid", id.PropNameOnData);
            PropertyDescriptor name = Assert.Single(desc.Properties, p => p.PropNameOnType == "Name");
            Assert.Equal("cname", name.PropNameOnData);
        }

        /// <summary>
        /// 无表特性时，默认表名取类型名
        /// </summary>
        [Fact]
        public void Build_NoTableAttribute_UsesTypeName()
        {
            EntityTypeDescriptor desc = EntityTypeModelManager.GetEntityTypeModel(
                typeof(PlainEntity), CreateOptions(withGetTableName: false), _ => { });

            Assert.Equal(nameof(PlainEntity), desc.TableName);
            Assert.Null(desc.TableSchema);
            Assert.Single(desc.Properties);
            Assert.Equal("Id", desc.Properties[0].PropNameOnData);
        }

        /// <summary>
        /// 标注 [NonFiled] 的属性/字段不会被映射为表字段
        /// </summary>
        [Fact]
        public void Build_NonFiledAttribute_SkipsMembers()
        {
            EntityTypeDescriptor desc = EntityTypeModelManager.GetEntityTypeModel(
                typeof(SkipEntity), CreateOptions(withGetTableName: false), _ => { });

            PropertyDescriptor keep = Assert.Single(desc.Properties);
            Assert.Equal("Keep", keep.PropNameOnType);
            Assert.DoesNotContain(desc.Properties, p => p.PropNameOnType == "SkipProp");
            Assert.DoesNotContain(desc.Properties, p => p.PropNameOnType == "SkipField");
        }

        /// <summary>
        /// buildOptions.TableName 显式指定时优先于特性/回调解析
        /// </summary>
        [Fact]
        public void Build_TableNameFromBuildOptions_TakesPrecedence()
        {
            EntityTypeDescriptor desc = EntityTypeModelManager.GetEntityTypeModel(
                typeof(PersonEntity), CreateOptions(), b => b.TableName = "custom_table");

            Assert.Equal("custom_table", desc.TableName);
            Assert.Null(desc.TableSchema);
        }

        /// <summary>
        /// GetTableName 返回 {schema}.{table} 格式时，能正确拆分为 schema 和表名
        /// </summary>
        [Fact]
        public void Build_GetTableNameReturnsSchemaTable_SplitsSchemaAndTable()
        {
            EntityTypeModelOptions options = CreateOptions();
            options.GetTableName = (type, opts) => "custom.c_person";

            EntityTypeDescriptor desc = EntityTypeModelManager.GetEntityTypeModel(
                typeof(CustomTableEntity), options, _ => { });

            Assert.Equal("c_person", desc.TableName);
            Assert.Equal("custom", desc.TableSchema);
        }

        /// <summary>
        /// buildOptions.Schema 显式指定时，不会被 GetTableName 中的 schema 覆盖
        /// </summary>
        [Fact]
        public void Build_SchemaFromBuildOptions_NotOverriddenByGetTableName()
        {
            EntityTypeModelOptions options = CreateOptions();
            options.GetTableName = (type, opts) => "x.y";

            EntityTypeDescriptor desc = EntityTypeModelManager.GetEntityTypeModel(
                typeof(CustomTableEntity), options, b => b.Schema = "fixed");

            Assert.Equal("fixed", desc.TableSchema);
            Assert.Equal("y", desc.TableName);
        }

        /// <summary>
        /// GetColumnName 回调可自定义列名映射规则
        /// </summary>
        [Fact]
        public void Build_GetColumnName_CustomMapping()
        {
            EntityTypeModelOptions options = CreateOptions();
            options.GetColumnName = (type, member, opts) => "COL_" + member.Name;

            EntityTypeDescriptor desc = EntityTypeModelManager.GetEntityTypeModel(
                typeof(PlainEntity), options, _ => { });

            Assert.Equal("COL_Id", desc.Properties[0].PropNameOnData);
        }

        /// <summary>
        /// 类型和属性上声明的 Attribute 会被收集到描述器中
        /// </summary>
        [Fact]
        public void Build_CollectsAttributesOnTypeAndProperty()
        {
            EntityTypeDescriptor desc = EntityTypeModelManager.GetEntityTypeModel(
                typeof(PersonEntity), CreateOptions(withGetTableName: false), _ => { });

            Assert.Contains(desc.Attrs, a => a is TableAttribute);

            PropertyDescriptor id = Assert.Single(desc.Properties, p => p.PropNameOnType == "Id");
            Assert.Contains(id.Attrs, a => a is ColumnAttribute);
        }

        /// <summary>
        /// buildOptions.Refer 会被写入描述器的 Refer 属性
        /// </summary>
        [Fact]
        public void Build_Refer_SetOnDescriptor()
        {
            object refer = new object();
            EntityTypeDescriptor desc = EntityTypeModelManager.GetEntityTypeModel(
                typeof(PlainEntity), CreateOptions(), b => b.Refer = refer);

            Assert.Same(refer, desc.Refer);
        }

        /// <summary>
        /// Refer 实现 IEntityTypeModel 时，改由该模型自身的 Build 方法生成描述
        /// </summary>
        [Fact]
        public void Build_ReferIsEntityTypeModel_UsesModelBuild()
        {
            EntityTypeDescriptor desc = EntityTypeModelManager.GetEntityTypeModel(
                typeof(PlainEntity), CreateOptions(), b => b.Refer = new CustomModel());

            Assert.Equal("from_model", desc.TableName);
            Assert.Equal(typeof(PlainEntity), desc.Type);

            PropertyDescriptor id = Assert.Single(desc.Properties);
            Assert.Equal("Id", id.PropNameOnType);
            Assert.Equal("id", id.PropNameOnData);
        }

        /// <summary>
        /// 配置 EntityTypeModelBuilderFactory 时，使用自定义的 IEntityTypeModelBuilder 生成模型
        /// </summary>
        [Fact]
        public void GetEntityTypeModel_UsesEntityTypeModelBuilderFactory()
        {
            EntityTypeModelOptions options = CreateOptions();
            options.EntityTypeModelBuilderFactory = () => new CustomBuilder();

            EntityTypeDescriptor desc = EntityTypeModelManager.GetEntityTypeModel(
                typeof(PlainEntity), options, _ => { });

            Assert.Equal("custom_builder", desc.TableName);
            Assert.Equal(typeof(PlainEntity), desc.Type);
        }

        /// <summary>
        /// 配置 EntityTypeModelFactory 时，使用自定义工厂方法生成模型
        /// </summary>
        [Fact]
        public void GetEntityTypeModel_UsesEntityTypeModelFactory()
        {
            EntityTypeModelOptions options = CreateOptions();
            options.EntityTypeModelFactory = (builder, opts, t) => builder.WithTableName("from_factory").WithType(t);

            EntityTypeDescriptor desc = EntityTypeModelManager.GetEntityTypeModel(
                typeof(PlainEntity), options, _ => { });

            Assert.Equal("from_factory", desc.TableName);
            Assert.Equal(typeof(PlainEntity), desc.Type);
        }

        /// <summary>
        /// configure 参数为 null 时抛出 ArgumentNullException
        /// </summary>
        [Fact]
        public void GetEntityTypeModel_NullConfigure_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                EntityTypeModelManager.GetEntityTypeModel(typeof(PlainEntity), CreateOptions(), null!));
        }
    }
}