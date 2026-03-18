namespace MNet.LTSQL.v1.SqlQueryStructs
{
    //严格按照顺序构建查询结构，方便后续的sql化
    public enum QueryStepSeq
    {
        From = 1,
        Join,
        //基本的查询语句
        Query,
        Where,
        GroupBy,
        Having,
        OrderBy,
        //投影
        Select,
        //分页
        Page,
        End = int.MaxValue
    }
}
