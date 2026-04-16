using System.Linq.Expressions;

namespace MNet.LTSQL.SqlQueryStructs
{
    public class OrderKeyPart
    {
        public bool Asc { get; set; }
        public Expression Key { get; set; }
    }
}
