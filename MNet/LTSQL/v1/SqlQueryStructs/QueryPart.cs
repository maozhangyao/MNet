using MNet.Utils;
using System;
using System.Collections.Generic;
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
    }
    


    //复杂查询结构数据源
    public class SqlQueryPart : QueryPart
    {
        //去重、分页等
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public bool Distinct { get; set; }
        public QueryStep Step { get; set; }


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
        public Expression SelectKey { get; set; }
        public Type NewType { get; set; }
    }
}
