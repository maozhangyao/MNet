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
    internal class DbPipe<T> : IDbSet<T> where T : class
    {
        public Expression<Func<T, bool>> WhereExpr { get; set; }
        public List<DbSetOrder> OrderExprs { get; set; } = new List<DbSetOrder>();

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

            Expression<Func<T, bool>> left = this.WhereExpr;
            Expression<Func<T, bool>> right = expr;
            this.WhereExpr = combine.Combine(left, right);
        }
    }

    /// <summary>
    /// 表示 T1 到 T2 的投影
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    internal class DbSelect<T1, T2> : DbPipe<T2> where T1 : class where T2 : class
    {
        public DbSelect(IDbSet<T1> src)
        {
            this.Source = src;
        }

        public IDbSet<T1> Source { get; set; }
        public Expression<Func<T1, T2>> SelectExpr { get; set; }
    }
}
