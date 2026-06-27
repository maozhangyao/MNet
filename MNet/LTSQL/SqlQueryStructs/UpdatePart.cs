using System.Linq.Expressions;

namespace MNet.LTSQL.SqlQueryStructs
{
    /// <summary>
    /// update 语句
    /// </summary>
    public class UpdatePart : NonQueryPart
    { 
        public Expression UpdateSet { get; set; }
    }
}
