using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNet.LTSQL.SqlTokenExtends
{
    /// <summary>
    /// 表示支持优先级计算的token接口
    /// </summary>
    public interface IPriorable
    {
        public bool IsPriority { get; }
        public IPriorable SetPriority(bool isPriority);
    }
}
