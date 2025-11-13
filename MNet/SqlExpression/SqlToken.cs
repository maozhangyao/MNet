namespace MNet.SqlExpression
{
    /// <summary>
    /// sql token
    /// </summary>
    public class SqlToken
    {
        public SqlToken()
        { }
        public SqlToken(string part, object obj)
        {
            this.SqlPart = part;
            this.Dynamic = obj;
        }

        /// <summary>
        /// 已经生成的sql
        /// </summary>
        public string SqlPart { get; set; }
        /// <summary>
        /// 动态值，还不能生成sql
        /// </summary>
        public object Dynamic { get; set; }
        public bool IsDynamic => this.Dynamic != null && this.SqlPart == null;
    }
}
