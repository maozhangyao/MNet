using MNet.LTSQL.SqlTokenExtends;
using System;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
    //sql 参数
    public class SqlParameterToken : SqlValueToken
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pName"></param>
        /// <param name="value"></param>
        /// <param name="valueType">值映射的类型, 注意类型不一定等于 value.GetType 。如：调用FirstOrDefault方法的场景</param>
        internal SqlParameterToken(string pName, object value, Type valueType)
        {
            this.Value = value;
            this.ParameterName = pName;
            this.ValueType = valueType;
        }


        //值
        public readonly object Value;
        //参数名
        public readonly string ParameterName;


        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitSqlParameterToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            return this;
        }
    }
}
