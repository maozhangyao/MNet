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
        /// <summary>
        /// 引用的SQL参数
        /// </summary>
        public List<SqlParamter> RefParameters { get; set; }
        /// <summary>
        /// 对外投射出的字段名
        /// </summary>
        public List<string> Fields { get; set; }
        public string Where { get; set; }
        public string OrderBy { get; set; }
        public string Select { get; set; }
        /// <summary>
        /// 实际的表名
        /// </summary>
        public string Table { get; set; }


        public string ToSql()
        {
            if (this.Define.IsEmpty)
                return "";
            if (this.Define.IsRoot)
                return this.Table;
            if (this.Define.IsInherit)
                return this.From.ToSql();

            string from = (this.From.Define.IsRoot ? this.From.ToSql() : $"({this.From.ToSql()})") + $" AS {this.From.Name}";
            string sql = $"SELECT {this.Select} FROM {from}";
            if (this.Where.IsNotEmpty())
                sql += $" WHERE {this.Where}";
            if (this.OrderBy.IsNotEmpty())
                sql += $" ORDER BY {this.OrderBy}";
            return sql;
        }
    }
}
