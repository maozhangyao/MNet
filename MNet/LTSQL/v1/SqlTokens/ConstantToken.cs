using System;

namespace MNet.LTSQL.v1.SqlTokens
{
    //一个 sql 硬编码的值
    public class ConstantToken : SQLValueToken
    {
        public ConstantToken(string val)
        {
            this.Value = val;
        }
        public ConstantToken(string val, Type type)
        {
            this.Value = val;
            this.ValueType = type;
        }

        public string Value { get; set; }

        public override void ToSql(LTSQLTokenContext context)
        {
            context.SQLBuilder.Append(context);
            //return this.Value;
        }
    }
}
