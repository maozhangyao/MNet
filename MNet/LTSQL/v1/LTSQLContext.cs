using MNet.LTSQL.v1.SqlTokens;
using System.Linq;
using System.Collections.Generic;

namespace MNet.LTSQL.v1
{
    public class LTSQLContext
    {
        //选项
        public LTSQLOptions Options { get; set; }
        //表名生成器
        public NameGenerator TableNameGenerator { get; set; }
        //sql参数名生成器
        public NameGenerator ParameterNameGenerator { get; set; }
        //
        public LTSQLTokenTranslaterSelector LTSQLTranslater { get; set; }
        public QuerySequence Root { get; set; }
        public TableAliasMapping TableAliasMapping { get; set; }



        //是否存在分组子句
        public bool GroupFlag { get; set; }
        public LTSQLToken GroupKey { get; set; }
        public LTSQLToken GroupElement { get; set; }
    }

    /// <summary>
    /// 表命名映射
    /// </summary>
    public class TableAliasMapping
    {
        public TableAliasMapping() 
        { }
        public TableAliasMapping(string prop)
        {
            this.PropName = prop;
            this.Props = new List<TableAliasMapping>(2);
        }
        public TableAliasMapping(string alias, string prop)
        {
            this.Alias = alias;
            this.PropName = prop;
        }



        //表命名
        public string Alias { get; set; }
        //属性名
        public string PropName { get; set; }
        public List<TableAliasMapping> Props { get; set; }

        public TableAliasMapping GetProp(string prop)
        {
            return this.Props?.FirstOrDefault(p => p.PropName == prop);
        }
    }
}
