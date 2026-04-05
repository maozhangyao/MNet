using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace MNet.LTSQL.v1.SqlTokens
{
    //一个 sql 硬编码的值
    public class ConstantToken : SqlValueToken
    {
        public ConstantToken(string val, Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            this.Value = val;
            this.ValueType = type;
        }


        public string Value { get; set; }

        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return Array.Empty<LTSQLToken>();
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitConstantToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            return this;
        }

        public static ConstantToken Create(object val, DbType db, Type? typeOfValue = null)
        {
            if (val == null && typeOfValue == null)
                throw new Exception($"值为null，无法推测出值的类型，请指定{nameof(typeOfValue)}参数");

            string str = DbUtils.ToSqlPart(val, db);
            return new ConstantToken(str, typeOfValue ?? val.GetType());
        }
    }
}
