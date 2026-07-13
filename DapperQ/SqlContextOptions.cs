using MNet.Kits;
using MNet.LTSQL;
using System;
using System.Data;

namespace DapperQ
{
    public class SqlContextOptions
    {
        public Action<string>? Log { get; set; }
        public LTSQLOptions? LTSQLOptions { get; set; }
        public IObjectRenting<IDbConnection> DbConnectionRenting { get; set; }
    }
}