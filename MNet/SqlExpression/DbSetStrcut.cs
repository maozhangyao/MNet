using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace MNet.SqlExpression
{
    //表示数据集的sql结构
    internal class DbSetStrcut
    {
        public DbSetStrcut()
        {
            this.OrderExprs = new List<DbSetOrder>();
        }


        //表示 from 中的数据集
        public DbSetStrcut From { get; set; }
        //表示 where
        public Expression WhereExpr { get; set; }
        //表示 order
        public List<DbSetOrder> OrderExprs { get; set; }
        //表示 select
        public Expression SelectExprs { get; set; }

        public bool IsEmpty => this.WhereExpr == null && this.OrderExprs.Count <= 0 && this.SelectExprs == null;
        public bool IsRoot => this.SelectExprs != null && this.WhereExpr == null && this.OrderExprs.Count <= 0;
    }
}
