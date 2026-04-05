using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// SQL命名语法
    /// </summary>
    public class AliasToken : LTSQLToken
    {
        public AliasToken() 
        { }
        public AliasToken(LTSQLToken item, string itemAlias)
        {
            Item = item;
            ItemAlias = itemAlias;
        }

        public LTSQLToken Item { get; set; }
        public string ItemAlias { get; set; }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return new[] { this.Item };
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitAliasToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            this.Item = this.Item.Visit(visitor);
            return this;
        }
    }
}
