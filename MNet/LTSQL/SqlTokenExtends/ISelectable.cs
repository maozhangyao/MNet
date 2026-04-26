using System;
using MNet.LTSQL.SqlTokens;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokenExtends
{
    public interface ISelectable
    {
        Type MappingType { get; }
        IEnumerable<FieldInfoToken> Fields { get; }
        
    }
}

