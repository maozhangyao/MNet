using MNet.LTSQL.v1.SqlTokens;
using System.Linq.Expressions;
using System.Reflection;

namespace MNet.LTSQL.v1
{
    public class LTSQLOptions
    {
        //数据库类型
        public DbType DbType { get; set; }
        //是否参数化
        public bool UseSqlParameter { get; set; }
        public MemberTranslaterSelector SQLTokenTranslaters { get; set; }
    }

    
}
