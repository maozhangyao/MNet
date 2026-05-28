using MNet.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MNet.LTSQL.SqlQueryStructs
{
    
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

        public override QueryPart CopyNew()
        {
            JoinPart _new = base.CopyNew() as JoinPart;
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
        public Expression SelectKey { get; set; }

        
        public override QueryPart CopyNew()
        {
            SqlQueryPart part = base.CopyNew() as SqlQueryPart;
            part.From = this.From?.CopyNew();
            return part;
        }
    }

}
