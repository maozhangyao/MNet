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
        //作用域参数
        private readonly Dictionary<string, LTSQLToken> ScopeParamters = new Dictionary<string, LTSQLToken>();


        public QueryPart Part { get; set; }
        //选项
        public LTSQLOptions Options { get; set; }
        //表名生成器
        public NameGenerator TableAliasGenerator { get; set; }
        //sql参数名生成器
        public NameGenerator ParameterNameGenerator { get; set; }
        //
        public LTSQLTokenTranslaterSelector LTSQLTranslater { get; set; }

        
        public LTSQLToken GetScopeParameter(string parameterName)
        {
            if (this.ScopeParamters.ContainsKey(parameterName))
                return this.ScopeParamters[parameterName];
            return null;
        }
        public void SetScopeParameter(string parameterName, LTSQLToken parameterToken)
        {
            if (this.ScopeParamters.ContainsKey(parameterName))
                this.ScopeParamters[parameterName] = parameterToken;
            else
                this.ScopeParamters.Add(parameterName, parameterToken);
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
