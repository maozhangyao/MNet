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
        public FromPart From { get; set; }

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

            part.From = NewFromPart(this.From);
            return part;
        }
        private FromPart NewFromPart(FromPart from)
        {
            if (from == null)
                return null;

            FromPart newFrom = new FromPart(from.Seq?.CopyNew());
            newFrom.Parent = this.NewFromPart(from.Parent);
            newFrom.JoinType = from.JoinType;
            newFrom.JoinKey1 = from.JoinKey1;
            newFrom.JoinKey2 = from.JoinKey2;
            newFrom.JoinKeyOn = from.JoinKeyOn;
            newFrom.JoinObject = from.JoinObject;
            return newFrom;
        }
    }
}
