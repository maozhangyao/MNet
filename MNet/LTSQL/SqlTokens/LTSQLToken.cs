using System;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlTokens
{
    //表示 sql 结构的一部分
    public abstract class LTSQLToken
    {
        public LTSQLToken()
        {
            this.Metadata = new Dictionary<string, object>();
        }

        //跨节点传递数据
        public Dictionary<string, object> Metadata { get; protected set; }


        protected internal virtual LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitToken(this);
        }
        protected internal virtual LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            return this;
        }

    }
}
