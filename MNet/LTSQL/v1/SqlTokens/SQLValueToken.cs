using System;

namespace MNet.LTSQL.v1.SqlTokens
{
    //表示能够映射到sql中的值，比如一个sql参数，sql变量，sql函数，sql表达式，表字段访问等等能够具有返回值的表达式
    public abstract class SQLValueToken : LTSQLToken
    {
        /// <summary>
        /// 值对应的c#类型
        /// </summary>
        public Type ValueType { get; set; }
    }
}
