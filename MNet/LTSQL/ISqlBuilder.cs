using MNet.LTSQL.SqlTokens;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace MNet.LTSQL
{
    /// <summary>
    /// 将LTSQLToken转化成sql
    /// </summary>
    public interface ISqlBuilder
    {
        void Build(LTSQLToken token, SqlBuilderContext context);
    }
}
