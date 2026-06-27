using System;

namespace MNet.LTSQL.SqlQueryStructs
{
    //可命名的数据序列
    public class QueryPart
    {
        protected QueryPart()
        {}

        public object Follow { get; set; }
        //命名
        public string Alias { get; set; }
        //an object type that mappting to table in a database
        public Type MappingType { get; set; }
        public virtual QueryPart CopyNew()
        {
            return this.MemberwiseClone() as QueryPart;
        }
    }
}
