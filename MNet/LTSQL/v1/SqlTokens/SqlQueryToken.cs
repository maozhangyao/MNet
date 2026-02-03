using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.v1.SqlTokens
{

    public class SqlQueryToken
    {
        public FromToken From { get; set; }
        public WhereToken Where { get; set; }
        public GroupToken Group { get; set; }
        public OrderToken Order { get; set; }
        public SelectToken Select { get; set; }
        public override string ToString()
        {
            return "";
        }
    }


}
