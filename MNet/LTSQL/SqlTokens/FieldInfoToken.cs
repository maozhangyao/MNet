using System;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 字段信息描述
    /// </summary>
    public class FieldInfoToken : BogusToken
    {
        public FieldInfoToken(LTSQLToken access, string field, Type accessType)
        {
            this.Field = field;
            this.Access = access;
            this.AccessType = accessType;
        }

        public string Field { get; set; }
        public LTSQLToken Access { get; set; }
        public Type AccessType { get; set; }
    }

}
