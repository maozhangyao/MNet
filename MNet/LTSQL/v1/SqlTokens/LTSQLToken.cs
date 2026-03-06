using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
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


        public abstract IEnumerable<LTSQLToken> GetChildren();
        protected internal virtual LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitToken(this);
        }
        protected internal virtual LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            var children = this.GetChildren();
            if (children != null)
            {
                foreach (LTSQLToken child in children)
                {
                    child.Visit(visitor);
                }
            }
            return this;
        }

    }
}
