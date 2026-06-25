using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.Attributes
{
    /// <summary>
    /// 定义映射的表格名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class QTableAttribute : Attribute
    {
        public QTableAttribute(string tname)
        {
            this.Name = tname;
        }

        public string Name { get; set; }
    }
}
