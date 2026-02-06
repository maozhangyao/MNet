using System.Collections.Generic;

namespace MNet.LTSQL.v1
{
    public class LTSQLContext
    {
        public LTSQLOptions Options { get; set; }
        //表名生成器
        public NameGenerator TableNameGenerator { get; set; }
        //sql参数名生成器
        public NameGenerator ParameterNameGenerator { get; set; }
        //
        public LTSQLTokenTranslaterSelector LTSQLTranslater { get; set; }

        //sql参数
        //public Dictionary<string, object> SqlParameters { get; set; } = new Dictionary<string, object>();

        public LTSQLTableNameMapping ObjectPrefix { get; set; }

        public QuerySequence Root { get; set; }


    }




    public class LTSQLTableNameMapping
    {
        public LTSQLTableNameMapping(Dictionary<string, string> p2t, HashSet<string> prefixs)
        {
            this._p2t = p2t;
            this._objPrefix = prefixs;
        }


        private Dictionary<string, string> _p2t;
        private HashSet<string> _objPrefix;


        public bool IsObjectPrefix(string objPrefix)
        {
            return this._objPrefix.Contains(objPrefix) && this.GetTableName(objPrefix) == null;
        }

        public string GetTableName(string accessPath)
        {
            return this._p2t.TryGetValue(accessPath, out string t) ? t : null;
        }
    }
}
