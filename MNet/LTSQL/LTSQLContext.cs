using MNet.LTSQL.SqlTokens;
using System.Linq;
using MNet.LTSQL.SqlQueryStructs;
using System.Runtime.CompilerServices;

namespace MNet.LTSQL
{
    public class LTSQLContext
    {
        public LTSQLContext() : this(null)
        { }
        public LTSQLContext(LTSQLOptions options)
        {
            this.Options = options;
            this.ParameterMgr = new ParameterScopeManager();
        }

        /// <summary>
        /// 翻译源
        /// </summary>
        public QueryPart Part { get; set; }
        /// <summary>
        /// 选项
        /// </summary>
        public LTSQLOptions Options { get; private set; }
        /// <summary>
        /// lambda 参数管理器
        /// </summary>
        public ParameterScopeManager ParameterMgr { get; }
        /// <summary>
        /// 表命名生成器
        /// </summary>
        public NameGenerator TableAliasGenerator { get; set; }
        /// <summary>
        /// sql参数名生成器
        /// </summary>
        public NameGenerator ParameterNameGenerator { get; set; }
        /// <summary>
        /// 表达式翻译扩展
        /// </summary>
        public LTSQLTokenTranslaterSelector LTSQLTranslater { get; set; }

        public static LTSQLContext Create(LTSQLOptions options)
        {
            return new LTSQLContext(options)
            {
                TableAliasGenerator = new NameGenerator(i => $"t{i}"),
                ParameterNameGenerator = new NameGenerator(i => $"p{i}"),
                LTSQLTranslater = new CombineTranslaterSelector(options?.SQLTokenTranslaters, LTSQLTokenTranslaterSelector.Default)
            };
        }
    }
}
