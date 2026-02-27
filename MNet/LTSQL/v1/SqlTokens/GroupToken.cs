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
        public LTSQLToken GroupKey { get; set; }
        /// <summary>
        /// 分组元素
        /// </summary>
        public LTSQLToken GroupElement { get; set; }

        public List<LTSQLToken> GroupByItems
        {
            get => this.GroupKey == null ? null : this.GroupKey is TupleToken tuple ? tuple.Props.ToList() : new List<LTSQLToken> { this.GroupKey };
        }


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return this.GroupByItems.ToArray();
        }
        public override void ToSql(LTSQLTokenContext context)
        {
            context.SQLBuilder.Append("GROUP BY ");
            bool comma = false;
            foreach (LTSQLToken item in GroupByItems)
            {
                if (comma)
                    context.SQLBuilder.Append(", ");
                comma = true;
                item.ToSql(context);
            }
        }
    }
}
