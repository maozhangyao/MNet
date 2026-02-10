namespace MNet.LTSQL.v1.SqlTokens
{
    //sql 参数
    public class SqlParameterToken : SQLValueToken
    {
        public SqlParameterToken() 
        { }
        public SqlParameterToken(string pName, object value)
        {
            this.Value = value;
            this.ParameterName = pName;
        }


        //值
        public object Value { get; set; }
        //参数名
        public string ParameterName { get; set; }
        public override string ToSql()
        {
            return this.ParameterName;
        }
    }
}
