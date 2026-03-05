using System;
using System.Linq;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class GroupToken : LTSQLToken
    {
        /// <summary>
        /// 分组依据
        /// </summary>
        public LTSQLToken[] GroupByItems { get; set; }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return this.GroupByItems.ToArray();
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitGroupToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            if (this.GroupByItems != null)
            {
                for (int i = 0; i < this.GroupByItems.Length; i++)
                {
                    LTSQLToken item = this.GroupByItems[i];
                    this.GroupByItems[i] = item.Visit(visitor);
                }
            }
            return this;
        }
    }
}
