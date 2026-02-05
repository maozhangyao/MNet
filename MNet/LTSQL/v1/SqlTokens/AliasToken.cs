using System;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// SQL 对象命名
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
