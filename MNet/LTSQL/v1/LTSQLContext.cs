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
        //sql参数
        public Dictionary<string, object> SqlParameters { get; set; } = new Dictionary<string, object>();

        public LTSQLTableNameMapping ObjectPrefix { get; set; }

        public QuerySequence Root { get; set; }
    }

    public class LTSQLTableNameMapping
    {
        public LTSQLTableNameMapping(Dictionary<string, string> p2t)
        {
            this._p2t = p2t;
        }


        private Dictionary<string, string> _p2t;

        public string GetTableName(string accessPath)
        {
            return this._p2t.TryGetValue(accessPath, out string t) ? t : null;
        }
    }
}
