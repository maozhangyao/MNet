using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MNet.SqlExpression
{
    /// <summary>
    /// 数据管道，表达对数据的操作，如过滤，排序，分组等等（表达sql结构)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DbPipe<T> : IDbSet<T>
    {
        protected DbPipe()
        { }

        public DbSetStrcut DbSet { get; protected set; }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)(new T[0])).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddWhere(Expression<Func<T, bool>> expr)
        {
            LogicExpresionCombine combine = new LogicExpresionCombine();

            Expression<Func<T, bool>> left = this.DbSet.WhereExpr as Expression<Func<T, bool>>;
            Expression<Func<T, bool>> right = expr;
            this.DbSet.WhereExpr = combine.Combine(left, right);
        }
        public void AddOrder<Tkey>(Expression<Func<T, Tkey>> expr, bool descending)
        {
            if (expr == null)
                throw new Exception("Order 排序表达式能不为空");

            this.DbSet.OrderExprs.Add(new DbSetOrder(expr, descending));
        }
        public void AddSelect<TResult>(Expression<Func<T, TResult>> expr)
        {
            if (expr == null)
                throw new Exception("Select 表达式不能为null");

            this.DbSet.SelectExprs = expr;
        }
    }
}
