using System;
using System.Collections.Generic;
using System.Net.Mime;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class SelectToken : LTSQLToken
    {
        public SelectToken()
        { }

        // 是否 select * 
        public bool AllFields { get; set; }
        public List<SelectItemToken> Fields { get; set; }


        public override void ToSql(LTSQLTokenContext context)
        {
            context.SQLBuilder.Append("SELECT ");

            if (this.AllFields)
            {
                context.SQLBuilder.Append("*");
            }
            else
            {
                bool comma = false;
                foreach (SelectItemToken item in this.Fields) 
                {
                    if (comma)
                        context.SQLBuilder.Append(", ");

                    comma = true;
                    item.ToSql(context);
                }
            }
        }
    }

    public class SelectItemToken : LTSQLToken
    {
        public SelectItemToken() 
        { }
        public SelectItemToken(LTSQLToken field, string fieldAlias)
        {
            Field = field;
            FieldAlias = fieldAlias;
        }

        public LTSQLToken Field { get; set; }
        public string FieldAlias { get; set; }

        public override void ToSql(LTSQLTokenContext context)
        {
            this.Field.ToSql(context);
            context.SQLBuilder.Append(' ');
            context.SQLBuilder.Append(this.FieldAlias);
        }
    }
}
