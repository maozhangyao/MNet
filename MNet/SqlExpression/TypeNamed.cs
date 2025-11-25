using System;

namespace MNet.SqlExpression
{
    /// <summary>
    /// 为类型命名，用于表达式参数名称绑定
    /// </summary>
    public class TypeNamed
    {
        public Type Type { get; set; }
        public string Name { get; set; }
    }
}
