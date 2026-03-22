using MNet.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MNet.LTSQL.v1.SqlQueryStructs
{
    //可命名的数据序列
    public class QueryPart
    {
        //命名
        public string Alias { get; set; }
        //an object type that mappting to table in a database
        public Type MappingType { get; set; }
        public virtual QueryPart CopyNew()
        {
            return new QueryPart()
            {
                Alias = Alias,
                MappingType = MappingType
            };
        }
    }
    
    //单表数据源
    public class TablePart : QueryPart
    {
        public TablePart(Type mappingType) 
        {
            this.MappingType = mappingType;
        }


        //指定的table名称(优先级最高)
        public string TableName { get; set; }
        //字段集合
        public string[] TableFields { get; set; }

        public override QueryPart CopyNew()
        {
            return new TablePart(this.MappingType)
            {
                Alias = this.Alias,
                TableName = this.TableName,
                TableFields = this.TableFields?.ToArray()
            };
        }
    }
    
    //联表数据源
    public class JoinPart : QueryPart
    {
        public JoinPart()
        {

        }
        public JoinPart(QueryPart q1, QueryPart q2)
        {
            this.MainQuery = q1;
            this.JoinQuery = q2;
        }



        public QueryPart MainQuery { get; set; }
        public QueryPart JoinQuery { get; set; }

        //join info
        //目前仅支持 left join
        public JoinType JoinType { get; set; }
        // 连接的左侧key选择
        public Expression JoinKey1 { get; set; }
        // 连接的右侧key选择(即当前类)
        public Expression JoinKey2 { get; set; }
        //Source1Key 和 Source2Key 做合并后得到的联接条件
        public Expression JoinKeyOn { get; set; }
        // 两个对象连接成一个对象： (t1, t2) => new { t1, t2 }
        public Expression JoinObject { get; set; }
        public string JoinKey1Prop { get; set; }
        public string JoinKey2Prop { get; set; }


        public override QueryPart CopyNew()
        {
            JoinPart _new = new JoinPart();
            _new.Alias = this.Alias;
            _new.MappingType = this.MappingType;
            _new.JoinType = this.JoinType;
            _new.JoinQuery = this.JoinQuery;
            _new.JoinKey1 = this.JoinKey1;
            _new.JoinKey2 = this.JoinKey2;
            _new.JoinKeyOn = this.JoinKeyOn;
            _new.JoinObject = this.JoinObject;
            _new.JoinKey1Prop = this.JoinKey1Prop;
            _new.JoinKey2Prop = this.JoinKey2Prop;

            _new.MainQuery = this.MainQuery?.CopyNew();
            _new.JoinQuery = this.JoinQuery?.CopyNew();
            return _new;
        }
    }

    //复杂查询结构数据源
    public class SqlQueryPart : QueryPart
    {
        //去重、分页等
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public bool Distinct { get; set; }
        public bool GroupFlag { get; set; }
        public QueryStepSeq Step { get; set; }


        //from
        public QueryPart From { get; set; }

        //where
        // 由于存在连表查询的情况，条件是一个表达式列表，默认是AND关系
        public List<Expression> Wheres { get; set; }

        //group by
        public Expression GroupKey { get; set; }
        public Expression GroupElement { get; set; }
        //having
        public List<Expression> Havings { get; set; }
        
        //order by
        public List<OrderKeyPart> Orders { get; set; }

        //select 
        public Type NewType { get; set; }
        public Expression SelectKey { get; set; }
        

        public override QueryPart CopyNew()
        {
            SqlQueryPart part = new SqlQueryPart();
            part.Step = this.Step;
            part.Alias = this.Alias;
            part.MappingType = this.MappingType;
            part.Skip = this.Skip;
            part.Take = this.Take;
            part.Distinct = this.Distinct;
            part.GroupFlag = this.GroupFlag;

            part.Wheres = this.Wheres?.ToList();
            part.GroupKey = this.GroupKey;
            part.GroupElement = this.GroupElement;

            part.Havings = this.Havings?.ToList();
            part.Orders = this.Orders?.ToList();
            part.SelectKey = this.SelectKey;
            part.NewType = this.NewType;

            part.From = this.From?.CopyNew();
            return part;
        }
    }
}
