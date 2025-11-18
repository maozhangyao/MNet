using System;
using System.Linq;
using System.Collections.Generic;
using MNet.Utils;

namespace MNet.SqlExpression
{
    /// <summary>
    /// SQL 结构描述
    /// </summary>
    public class SqlDescriptor
    {
        public SqlDescriptor()
        {
            this.Fields = new List<string>();
            this.RefParameters = new List<SqlParamter>();
        }


        /// <summary>
        /// SQL 结构描述初始定义
        /// </summary>
        internal DbSetStrcut Define { get; set; }
        /// <summary>
        /// 命名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 数据源
        /// </summary>
        public SqlDescriptor From { get; set; }
        /// <summary>
        /// 引用到其他的数据源
        /// </summary>
        public List<SqlDescriptor> Refs { get; set; }
        public string Where { get; set; }
        public string OrderBy { get; set; }
        public string Select { get; set; }
        
        /// <summary>
        /// 引用的SQL参数
        /// </summary>
        public List<SqlParamter> RefParameters { get; set; }
        /// <summary>
        /// 对外投射出的字段名
        /// </summary>
        public List<string> Fields { get; set; }

        public string ToSql()
        {
            if (this.Define.IsRoot)
                return this.Name;

            string sql = "";
            if (this.Select.IsNotEmpty())
            {

            }

            return sql;
        }
    }
}
