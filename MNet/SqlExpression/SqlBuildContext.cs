using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.SqlExpression
{
    public class SqlBuildContext
    {
        public SqlBuildContext()
        {
            this.RefTableCount = 0;
            this.RefParamCount = 0;
            this.Descriptors = new List<SqlDescriptor>(5);
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
        /// 所有涉及到的SQL参数
        /// </summary>
        public List<SqlParamter> RefParameters { get; set; }
        /// <summary>
        /// 所有需要生成的SQL
        /// </summary>
        public List<SqlDescriptor> Descriptors { get; set; }
    }
}
