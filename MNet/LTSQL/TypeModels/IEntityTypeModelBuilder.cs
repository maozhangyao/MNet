using System;

namespace MNet.LTSQL.TypeModels
{
    /// <summary>
    /// 生成指定类型的实体类型模型接口
    /// </summary>
    public interface IEntityTypeModelBuilder
    {
        EntityTypeDescriptor Build(Type entityType, EntityTypeModelBuildOptions options);
    }
}
