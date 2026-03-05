using MNet.Utils;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MNet.LTSQL.v1.SqlQueryStructs
{
    //可命名的数据序列
    public class Sequence
    {
        //字段集合
        public string[] Fields { get; set; }
        //命名
        public string Alias { get; set; }
        //an object type that mappting to table in a database
        public Type Type { get; set; }
    }
    //单表数据源
    public class SimpleSequence : Sequence
    {
        public SimpleSequence(Type mappingType) 
        {
            this.Type = mappingType;
        }
    }
    //复杂查询结构数据源
    public class QuerySequence : Sequence
    {
        public QueryStep Step { get; set; }
        //from 结构：
        //1. from
        //2. from join
        public FromPart F { get; set; }

        //where
        // 由于存在连表查询的情况，条件是一个表达式列表，默认是AND关系
        public List<Expression> Wheres { get; set; }

        //group by
        public Expression GroupKey { get; set; }
        public Expression GroupElement { get; set; }
        //public HavingUnit Having { get; set; }
        
        //order by
        public List<OrderKey> Orders { get; set; }

        //select 
        public Expression SelectKey { get; set; }
        public Type NewType { get; set; }

        //
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public bool Distinct { get; set; }

        internal bool IsSimpleSelect()
        {
            //简单查询，除了from子句，没有其余的子句了
            return this.Step == QueryStep.From;
        }
        internal QuerySequence UnWrap()
        {
            //拆包：属于简单查询，但对于from子句是内联查询，则直接返回内联查询
            if (!this.IsSimpleSelect())
                return this;
            if (this.Step > QueryStep.From)
                return this;

            if (this.F.Seq is QuerySequence q)
                return q.UnWrap();
            
            return this;
        }
    }

    //严格按照顺序构建查询结构，方便后续的sql化
    public enum QueryStep
    {
        From = 1,
        Join,
        Where,
        GroupBy,
        OrderBy,
        Select
    }

    public class OrderKey
    {
        public bool Asc { get; set; }
        public Expression Key { get; set; }
    }


    public class FromPart
    {
        public FromPart Parent { get; set; }
        public Sequence Seq { get; set; }

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
