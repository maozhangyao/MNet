namespace MNet.LTSQL.v1
{
    public class LTSQLContext
    {
        //表名生成器
        public NameGenerator TableNameGenerator { get; set; }
        //sql参数名生成器
        public NameGenerator ParameterNameGenerator { get; set; }

        public Sequence Root { get; set; }
    }
}
