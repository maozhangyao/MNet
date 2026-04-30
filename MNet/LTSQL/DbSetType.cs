namespace MNet.LTSQL
{
    /// <summary>
    /// 支持的集合操作类型
    /// </summary>
    public enum DbSetType
    {
        /// <summary>
        /// 并集
        /// </summary>
        Union,
        /// <summary>
        /// 交集
        /// </summary>
        Intersect,
        /// <summary>
        /// 差集
        /// </summary>
        Except
    }
}