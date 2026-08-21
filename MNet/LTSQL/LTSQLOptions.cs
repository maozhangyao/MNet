using System;
using System.Linq.Expressions;
using System.Reflection;
using MNet.LTSQL.SqlTokens;
using MNet.LTSQL.TypeModels;

namespace MNet.LTSQL
{
    public class LTSQLOptions
    {
        /// <summary>
        /// 一个无意义的id，也许可以用来区别场景
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        public DbTypes DbType { get; set; }
        /// <summary>
        /// 是否生成参数化sql， 默认true
        /// </summary>
        public bool UseSqlParameter { get; set; } = true;
        /// <summary>
        /// 当识别到null参数时，是否禁止自动处理null值等
        ///  value1 == null   转换为 value1 is NULL
        ///  value2 != null   转换为 value2 is not NULL
        /// </summary>
        public bool DisNullable { get; set; }

        // TODO token optimize interface design

        public EntityTypeModelOptions EntityTypeModelOptions { get; set; }
        public LTSQLTokenTranslaterSelector SQLTokenTranslaters { get; set; }
        public Action<LTSQLOptions, SqlBuilderOptions> ConfigSqlBuilderOptions { get; set; }
        
    }
}
