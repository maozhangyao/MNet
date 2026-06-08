using MNet.LTSQL.SqlTokens;
using System.Linq;
using System.Collections.Generic;
using MNet.LTSQL.SqlQueryStructs;
using System.Collections;
using MNet.LTSQL.Objects;
namespace MNet.LTSQL
{
    public class LTSQLContext
    {
        //选项
        public LTSQLOptions Options { get; set; }
        //表名生成器
        public NameGenerator TableAliasGenerator { get; set; }
        //sql参数名生成器
        public NameGenerator ParameterNameGenerator { get; set; }
        //
        public LTSQLTokenTranslaterSelector LTSQLTranslater { get; set; }

        public SqlQueryPart Root { get; set; }
        public string RootParameterName { get; private set; }
        public LTSQLToken RootParameterToken { get; private set; }

        public void SetRootParameter(string parameterName, LTSQLToken parameterToken)
        {
            this.RootParameterName = parameterName;
            this.RootParameterToken = parameterToken;
        }

        public static LTSQLContext Create(LTSQLOptions options)
        {
            return new LTSQLContext()
            {
                Options = options,
                TableAliasGenerator = new NameGenerator(i => $"t{i}"),
                ParameterNameGenerator = new NameGenerator(i => $"p{i}"),
                LTSQLTranslater = new CombineTranslaterSelector(options?.SQLTokenTranslaters, LTSQLTokenTranslaterSelector.Default)
            };
        }
    }
}
