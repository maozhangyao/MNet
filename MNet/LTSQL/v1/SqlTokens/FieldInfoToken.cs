namespace MNet.LTSQL.v1.SqlTokens
{
    /// <summary>
    /// 字段信息描述
    /// </summary>
    public class FieldInfoToken : BogusToken
    {
        public FieldInfoToken(LTSQLToken obj, string field)
        {
            this.Field = field;
            this.Object = obj;
        }

        public string Field { get; set; }
        public LTSQLToken Object { get; set; }
    }

}
