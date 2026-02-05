using System;

namespace MNet.LTSQL.v1.SqlTokens
{
    public class ObjectAccessToken : LTSQLToken
    {
        public ObjectAccessToken() { }

        //表名
        public AliasToken Owner { get; set; }
        //表字段
        public AliasToken Field { get; set; }

        public override string ToSql()
        {
            return "Field";
        }
    }

    //sql 参数
    public class SqlParameterToken : LTSQLToken
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

    //一个 sql 硬编码的值
    public class ConstantToken : LTSQLToken
    {
        public ConstantToken(string val)
        {
            this.Value = val;
        }


        public string Value { get; set; }
        public override string ToSql()
        {
            return this.Value;
        }
    }
}
