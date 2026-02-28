using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 表达出用分隔符分割的一组token集合
    /// </summary>
    public class TokenItemListToken : LTSQLToken
    {
        public TokenItemListToken()
        {

        }
        public TokenItemListToken(params LTSQLToken[] tokens)
        {
            this.Items = tokens;
        }
        public TokenItemListToken(IEnumerable<LTSQLToken> tokens)
        {
            this.Items = tokens?.ToArray();
        }


        /// <summary>
        /// 分隔符号，一般是逗号
        /// </summary>
        public string Separator { get; set; } = ", ";
        public LTSQLToken[] Items { get; set; }



        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return this.Items.ToArray();
        }
        public override void ToSql(LTSQLTokenContext context)
        {
            if (this.Items == null)
                return;

            for(int i = 0; i < this.Items.Length; i++)
            {
                if (i > 0)
                    context.SQLBuilder.Append(this.Separator);

                LTSQLToken item = this.Items[i];
                item.ToSql(context);
            }

        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return base.Visit(visitor);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            if(this.Items != null)
            {
                for(int i = 0; i < this.Items.Length; i++)
                {
                    LTSQLToken item = this.Items[i];
                    this.Items[i] = item.Visit(visitor);
                }
            }
            return this;
        }
    }
}
