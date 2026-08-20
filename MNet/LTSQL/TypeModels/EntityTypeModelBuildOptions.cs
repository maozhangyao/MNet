using System;
using System.Reflection;

namespace MNet.LTSQL.TypeModels
{
    /// <summary>
    /// 实体类型模型构造选项
    /// </summary>
    public class EntityTypeModelBuildOptions
    {
        /// <summary>
        /// 实体的类型
        /// </summary>
        public Type EntityType { get; set; }
        /// <summary>
        /// 模型面向的数据库
        /// </summary>
        public DbTypes Database { get; set; }
        /// <summary>
        /// 类型的实例对像，可以通过不同的实例对象做同一个Type的差异化Model
        /// </summary>
        public object? Refer { get; set; }
        /// <summary>
        /// 显示提供的 table schema
        /// </summary>
        public string? Schema { get; set; }
        /// <summary>
        /// 显示提供映射的表格名称
        /// </summary>
        public string? TableName { get; set; }
        public EntityTypeDescriptor Default { get; set; }
    }
}
