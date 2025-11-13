namespace MNet.SqlExpression
{
    /// <summary>
    /// SQL 参数
    /// </summary>
    public class SqlParamter
    {
        public SqlParamter() { }
        public SqlParamter(string name, object value) 
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; set; }
        public object Value { get; set; }
    }
}
