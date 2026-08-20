using System;
using System.Collections.Generic;
using System.Reflection;

namespace MNet.LTSQL.TypeModels
{
    /// <summary>
    /// 属性描述
    /// </summary>
    public class PropertyDescriptor
    {
        public PropertyDescriptor() 
        {
            this.Attrs = new List<Attribute>();
        }

        /// <summary>
        /// 字段顺序
        /// </summary>
        public int Position { get; set; }
        /// <summary>
        /// 属性成员
        /// </summary>
        public MemberInfo Property { get; set; }
        /// <summary>
        /// 属性类型
        /// </summary>
        public Type PropertyType { get; set; }
        /// <summary>
        /// 属性名
        /// </summary>
        public string PropNameOnType { get; set; }
        /// <summary>
        /// 数据库列名
        /// </summary>
        public string PropNameOnData { get; set; }
        /// <summary>
        /// 应用在属性上的特性
        /// </summary>
        public List<Attribute> Attrs { get; }
    }
}
