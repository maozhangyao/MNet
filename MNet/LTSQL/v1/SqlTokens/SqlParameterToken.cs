using System;

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
        public override void ToSql(LTSQLTokenContext context)
        {
            if (context?.Options?.UseSqlParameter ?? true)
            {
                if (!this.ParameterName.StartsWith("@"))
                    context.SQLBuilder.Append('@');
                context.SQLBuilder.Append(this.ParameterName);
            }
            else
            {
                throw new Exception("对象实例转换为sql字面量暂未支持");
            }
        }
    }
}
