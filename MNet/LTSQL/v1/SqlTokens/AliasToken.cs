using System;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 命名对象
    /// </summary>
    public class AliasToken : ValueToken
    {
        public AliasToken()
        { }
        public AliasToken(string alias)
        {
            this.Alias = alias;
        }

        public string Alias { get; set; }
        //关键字转义，如mysql中使用反引号包裹对象名，sqlserver 中使用中括号包裹对象名
        public bool KeyWorld { get; set; } = true;

        public override void ToSql(LTSQLTokenContext context)
        {
            context.SQLBuilder.Append(this.Alias);
        }
    }
}
