namespace MNet.LTSQL.v1
{
    //sql 作用域
    public class LTSQLScope
    {
        public LTSQLScope()
        { }

        public LTSQLScope Parent { get; private set; }
        public LTSQLContext Context { get; set; }


        public LTSQLScope NewScope()
        {
            return new LTSQLScope()
            {
                Parent = this,
                Context = new LTSQLContext()
            };
        }
    }
}
