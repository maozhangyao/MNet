using System;

namespace MNet.LTSQL
{
    /// <summary>
    /// 默认的LTSQL options配置
    /// </summary>
    public class LTSQLOptionsSetting
    {
        public static Func<LTSQLOptions> OptionCreate { get; set; }
        public static Func<LTSQLOptions, SqlBuilderContext> SqlBuildContextCreate { get; set; }


        internal static LTSQLOptions GetOptions()
        {
            if (OptionCreate != null)
                return OptionCreate();

            return null;
        }
        internal static SqlBuilderContext GetSqlBuildContext(LTSQLOptions options)
        {
            if (SqlBuildContextCreate != null)
                return SqlBuildContextCreate(options);
            
            SqlBuilderContext ctx = new SqlBuilderContext();
            ctx.DbType = options.DbType;
            ctx.UseParameter = options.UseSqlParameter;
            ctx.SqlWriterFactory = () => new LTSQLWriter(true);
            return ctx;
        }
    }
}
