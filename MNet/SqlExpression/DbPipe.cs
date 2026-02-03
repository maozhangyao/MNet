using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;

namespace MNet.SqlExpression
{
    /// <summary>
    /// 数据管道，表达对数据的操作，如过滤，排序，分组等等（表达sql结构)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DbPipe<T> : IDbSet<T>
    {
        protected DbPipe()
        {
            this.Namer = new NamedCreator(i => $"p{i}");
        }

        public DbSetStrcut DbSet { get; protected set; }
        public NamedCreator Namer { get; set; }


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
            TypeNamed named = this.DbSet.FromNamed;
            if (this.DbSet.WhereExpr == null)
            {
                this.DbSet.WhereExpr = expr.RenameParameter(named.Name);
                return;
            }
            else
            {
                Expression<Func<T, bool>> left = this.DbSet.WhereExpr as Expression<Func<T, bool>>;
                Expression<Func<T, bool>> right = expr;
                Expression<Func<T, bool>> where = left.And(named.Name, right);
                this.DbSet.WhereExpr = where;
            }
        }
        public void AddOrder<Tkey>(Expression<Func<T, Tkey>> expr, bool descending)
        {
            if (expr == null)
                throw new Exception("Order 排序表达式能不为空");

            TypeNamed named = this.DbSet.FromNamed;
            this.DbSet.OrderExprs.Add(new DbSetOrder(expr.RenameParameter(named.Name), descending));
        }
        public void AddSelect<TResult>(Expression<Func<T, TResult>> expr)
        {
            if (expr == null)
                throw new Exception("Select 表达式不能为null");

            TypeNamed named = this.DbSet.FromNamed;
            this.DbSet.SelectExprs = named != null ? expr.RenameParameter(named.Name) : expr;
        }

        //创建一个命名
        public TypeNamed CreateNamed(Type t)
        {
            return new TypeNamed
            {
                Name = this.Namer.Next(),
                Type = t
            };
        }
    }


    //
    public class DbGrouping<Tkey, TElement> : IGrouping<Tkey, TElement>
    {
        public Tkey Key { get; set; }

        public IEnumerator<TElement> GetEnumerator()
        {
            return ((IEnumerable<TElement>)Array.Empty<TElement>()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
