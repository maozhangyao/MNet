using System.Linq.Expressions;

namespace MNet.LTSQL.SqlQueryStructs
{
    public class NonQueryPart : QueryPart
    {
        public object Refer { get; set; }
        public string Schema { get; set; }
        public string TableName { get; set; }
        public Expression Where { get; set; }
    }
}
