namespace MNet.LTSQL.v1.SqlQueryStructs
{
    //严格按照顺序构建查询结构，方便后续的sql化
    public enum QueryStep
    {
        From = 1,
        Join,
        //基本的查询语句
        Query,
        Where,
        GroupBy,
        Having,
        OrderBy,
        Page,
        Select
    }
}
