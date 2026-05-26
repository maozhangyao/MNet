using System;
using MNet.LTSQL.SqlTokens;
using System.Collections.Generic;
using MNet.LTSQL.Objects;

namespace MNet.LTSQL.SqlTokenExtends
{
    public interface ISelectable : ITupleable
    {
        TableDescriptor Table { get; }
    }
}

