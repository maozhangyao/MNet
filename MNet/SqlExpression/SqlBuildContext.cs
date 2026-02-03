using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MNet.SqlExpression
{
    public class SqlBuildContext
    {
        public SqlBuildContext()
        {
            //this.RefTableCount = 0;
            //this.RefParamCount = 0;

            this.TableNamer = new NamedCreator(i => $"t{i.ToString().PadLeft(2, '0')}");
            this.SqlParamNamer = new NamedCreator(i => $"@p{i.ToString().PadLeft(3, '0')}");
            this.ParameterNamer = new NamedCreator(i => $"arg{i}");

            this.Descriptors = new List<SqlDescriptor>(5);
            this.RefParameters = new List<SqlParamter>(32);
        }


        /// <summary>
        /// 配置
        /// </summary>
        public SqlOptions Options { get; set; }
        /// <summary>
        /// 所有涉及到的SQL参数
        /// </summary>
        public List<SqlParamter> RefParameters { get; set; }
        /// <summary>
        /// 所有需要生成的SQL结构
        /// </summary>
        public List<SqlDescriptor> Descriptors { get; set; }


        /// <summary>
        /// 当前的sql作用域
        /// </summary>
        public SqlScope SqlScope { get; set; }

        /// <summary>
        /// 表名生成器
        /// </summary>
        public NamedCreator TableNamer { get; set; }
        /// <summary>
        /// SQL  参数名生成器
        /// </summary>
        public NamedCreator SqlParamNamer { get; set; }
        /// <summary>
        /// 表达式参数名生成器
        /// </summary>
        public NamedCreator ParameterNamer { get; set; }
    }
}
