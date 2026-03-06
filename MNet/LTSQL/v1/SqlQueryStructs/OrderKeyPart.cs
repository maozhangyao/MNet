using System.Linq.Expressions;

namespace MNet.LTSQL.v1.SqlQueryStructs
{
    public class OrderKeyPart
    {
        public bool Asc { get; set; }
        public Expression Key { get; set; }
    }
}
