using System;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 命名对象
    /// </summary>
    public class AliasToken : LTSQLToken
    {
        public AliasToken()
        { }
        public AliasToken(string alias)
        {
            this.Alias = alias;
        }


        //
        public Type Type { get; set; }
        public string Alias { get; set; }


        public override string ToSql()
        {
            return Alias;
        }
    }
}
