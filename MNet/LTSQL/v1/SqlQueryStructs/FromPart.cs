using System.Linq.Expressions;

namespace MNet.LTSQL.v1.SqlQueryStructs
{
    public class FromPart
    {
        public FromPart() 
        { }
        public FromPart(QueryPart seq)
        {
            this.Seq = seq;
        }
        public FromPart(FromPart parent)
        {
            this.Parent = parent;
        }
        

        public FromPart Parent { get; set; }
        public QueryPart Seq { get; set; }

        //join info
        //目前仅支持 left join
        public string JoinType { get; set; }
        // 连接的左侧key选择
        public Expression JoinKey1 { get; set; }
        // 连接的右侧key选择(即当前类)
        public Expression JoinKey2 { get; set; }
        //Source1Key 和 Source2Key 做合并后得到的联接条件
        public Expression JoinKeyOn { get; set; }
        // 两个对象连接成一个对象： (t1, t2) => new { t1, t2 }
        public Expression JoinObject { get; set; }
        
    }
}
