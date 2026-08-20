using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MNet.LTSQL.TypeModels
{
    /// <summary>
    /// 实体类型描述：实体类行与数据库表的映射关系
    /// </summary>
    public class EntityTypeDescriptor
    {
        public EntityTypeDescriptor()
        {
            this.Properties = new List<PropertyDescriptor>();
            this.Attrs = new List<Attribute>();
        }

        /// <summary>
        /// 数据库实体类型
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// 引用对象(如实体实例)
        /// </summary>
        public object Refer { get; set; }
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 表架构
        /// </summary>
        public string TableSchema { get; set; }
        /// <summary>
        /// 应用在实体类型的特性
        /// </summary>
        public List<Attribute> Attrs { get; set; }
        /// <summary>
        /// 实体上的属性
        /// </summary>
        public List<PropertyDescriptor> Properties { get; }

        public void AddProperty(PropertyDescriptor prop)
        {
            this.Properties.Add(prop);
        }
    }
}
