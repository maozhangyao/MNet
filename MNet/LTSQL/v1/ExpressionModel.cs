using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MNet.LTSQL.v1
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
        //from 结构： 
        //1. from
        //2. from join
        public FromUnit From { get; set; }
        public WhereUnit Where { get; set; }
        public GroupUnit Group { get; set; }
        public HavingUnit Having { get; set; }
        public OrderUnit Order { get; set; }
        public SelectUnit Select { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }


        internal bool IsSimpleSelect()
        {
            //简单查询，除了from子句，没有其余的子句了
            return this.From != null
                && this.Where == null
                && this.Group == null
                && this.Having == null
                && this.Order == null
                && this.Skip == null
                && this.Take == null;
        }
        internal QuerySequence UnWrap()
        {
            //拆包：属于简单查询，但对于from子句是内联查询，则直接返回内联查询
            if (!this.IsSimpleSelect())
                return this;

            return this.IsSimpleSelect() && this.From.Source is QuerySequence complex ? complex : this;
        }
    }


    public class FromUnit
    {
        public Sequence Source { get; set; }
    }
    public class FromJoinUnit : FromUnit
    {
        public FromUnit From { get; set; }

        // 连接的左侧key选择
        public Expression Source1Key { get; set; }
        // 连接的右侧key选择(即当前类)
        public Expression Source2Key { get; set; }
        // 两个对象连接成一个对象： (t1, t2) => new { t1, t2 }
        public Expression JoinExpr { get; set; }
    }


    public class WhereUnit
    {
        // 由于存在连表查询的情况，条件是一个表达式列表，默认是AND关系
        public List<Expression> Conditions { get; set; }
    }
    public class GroupUnit
    {
        public Expression GroupKeys { get; set; }
    }
    public class HavingUnit : WhereUnit
    { }
    public class OrderUnit
    {
        // bool表示是否是降序排列
        public List<KeyValuePair<Expression, bool>> OrderKeys { get; set; }
    }
    public class SelectUnit
    {
        public Expression SelectKey { get; set; }
        public Type NewType { get; set; }
    }
}
