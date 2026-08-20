namespace MNet.LTSQL.TypeModels
{
    /// <summary>
    /// 支持构造实体类型模型接口
    /// </summary>
    public interface IEntityTypeModel
    {
        void Build(EntityTypeModelBuilder builder, EntityTypeModelBuildOptions options);
    }
}
