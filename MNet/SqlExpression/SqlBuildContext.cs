using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.SqlExpression
{
    public class SqlBuildContext
    {
        public SqlBuildContext()
        {
            this.RefParameters = new List<SqlParamter>(32);
        }

        /// <summary>
        /// 配置
        /// </summary>
        public SqlOptions Options { get; set; }
        /// <summary>
        /// 引用 sql 参数名的计数
        /// </summary>
        public int RefParamCount { get; set; }
        /// <summary>
        /// 引用表名的计数
        /// </summary>
        public int RefTableCount { get; set; }
        /// <summary>
        /// 需要生成的 SQL
        /// </summary>
        public SqlDescriptor Descriptor { get; set; }
        /// <summary>
        /// 所有涉及到的SQL参数
        /// </summary>
        public List<SqlParamter> RefParameters { get; set; }
    }

    /// <summary>
    /// SQL 结构描述
    /// </summary>
    public class SqlDescriptor
    {
        public SqlDescriptor From { get; set; }
        /// <summary>
        /// 引用到其他的数据源
        /// </summary>
        public List<SqlDescriptor> Refs { get; set; }
        public string Where { get; set; }
        public string OrderBy { get; set; }
        public string Select { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 引用的SQL参数
        /// </summary>
        public List<SqlParamter> RefParameters { get; set; }
    }
}
