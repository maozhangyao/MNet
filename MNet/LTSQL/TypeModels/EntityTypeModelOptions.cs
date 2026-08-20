using System;
using System.Reflection;

namespace MNet.LTSQL.TypeModels
{
    /// <summary>
    /// 模型类型构造对外保留扩展
    /// </summary>
    public class EntityTypeModelOptions
    {
        /// <summary>
        /// IEntityTypeModelBuilder 接口工厂
        /// </summary>
        public Func<IEntityTypeModelBuilder> EntityTypeModelBuilderFactory { get; set; }
        /// <summary>
        /// 获取表名称.支持返回如此格式：{schema}.{table}
        /// </summary>
        public Func<Type, EntityTypeModelBuildOptions, string> GetTableName { get; set; }
        /// <summary>
        /// 获取表字段名称
        /// </summary>
        public Func<Type, MemberInfo, EntityTypeModelBuildOptions, string> GetColumnName { get; set; }
        /// <summary>
        /// 生成指定类型的数据实体模型
        /// </summary>
        public Action<EntityTypeModelBuilder, EntityTypeModelBuildOptions, Type> EntityTypeModelFactory { get; set; }
    }
}
