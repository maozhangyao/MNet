using System;

namespace MNet.LTSQL.Attributes
{
    /// <summary>
    /// 定义映射字段的名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class QColumnAttribute : Attribute
    {
        public QColumnAttribute(string column)
        {
            this.Name = column;
        }

        public string Name { get; set; }
    }
}
