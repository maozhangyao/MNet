using System.Linq.Expressions;

namespace MNet.LTSQL.SqlQueryStructs
{
    public class NonQueryPart : QueryPart
    {
        public Expression Where { get; set; }
    }
}
