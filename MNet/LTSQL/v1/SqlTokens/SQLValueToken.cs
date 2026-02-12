using System;

namespace MNet.LTSQL.v1.SqlTokens
{
    //表示能够求出一个sql值，比如一个sql参数，sql变量，sql函数，sql表达式，表字段访问等等能够具有返回值的表达式
    public abstract class SQLValueToken : ValueToken
    { }
}
