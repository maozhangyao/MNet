using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.Objects
{
    public class TableMetadata
    {
        public object? Refer { get; set; }
        public string? Table { get; set; }
        public string? Schema { get; set; }
    }
}
