using System;

namespace MNet.LTSQL
{
    /// <summary>
    /// 默认的LTSQL options配置
    /// </summary>
    public class LTSQLOptionsSetting
    {
        public static Func<LTSQLOptions> OptionCreator { get; set; }
        public static Func<LTSQLOptions, SqlBuilderOptions> SqlBuildContextCreator { get; set; }


        internal static LTSQLOptions GetOptions()
        {
            if (OptionCreator != null)
                return OptionCreator();

            return null;
        }
        internal static SqlBuilderOptions GetSqlBuildContext(LTSQLOptions options)
        {
            if (SqlBuildContextCreator != null)
                return SqlBuildContextCreator(options);
            
            SqlBuilderOptions ctx = new SqlBuilderOptions();
            ctx.DbType = options.DbType;
            ctx.UseParameter = options.UseSqlParameter;
            ctx.SqlWriterFactory = () => new LTSQLWriter(true);
            return ctx;
        }
    }
}
