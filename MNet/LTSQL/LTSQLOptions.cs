using System;
using System.Linq.Expressions;
using System.Reflection;
using MNet.LTSQL.SqlTokens;

namespace MNet.LTSQL
{
    public class LTSQLOptions
    {
        //数据库类型
        public DbType DbType { get; set; }
        //是否参数化， 默认true
        public bool UseSqlParameter { get; set; } = true;
        //当识别到null参数时，是否禁止自动处理null值等式，比如：
        // value1 == null   转换为 value1 is NULL
        // value2 != null   转换为 value2 is not NULL
        public bool DisNullable { get; set; }

        public LTSQLTokenTranslaterSelector SQLTokenTranslaters { get; set; }
        //获取表名称
        public Func<LTSQLMemberContext, string> GetTableName { get; set; }
        //获取表字段名称
        public Func<LTSQLMemberContext, string> GetColumnName { get; set; }
    }
}
