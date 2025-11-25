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
            this.TypeNamed = new Dictionary<string, TypeNamed>();
        }
        public DbSetStrcut(Type t): this()
        {
            this.Type = t;
        }


        //表示 from 中的数据集
        public DbSetStrcut From { get; set; }
        // from 命名
        public TypeNamed FromNamed { get; set; }

        //表示 where
        public Expression WhereExpr { get; set; }
        //表示 order
        public List<DbSetOrder> OrderExprs { get; set; }
        //表示 select
        public Expression SelectExprs { get; set; }
        //绑定的类型
        public Type Type { get; set; }
        public Dictionary<string, TypeNamed> TypeNamed { get; set; }


        public bool IsRoot => this.SelectExprs != null && this.WhereExpr == null && this.OrderExprs.Count <= 0;
        public bool IsEmpty => this.WhereExpr == null && this.OrderExprs.Count <= 0 && this.SelectExprs == null && this.From == null;
        public bool IsInherit => this.WhereExpr == null && this.OrderExprs.Count <= 0 && this.SelectExprs == null && this.From != null;
    }
}
