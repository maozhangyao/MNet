using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MNet.LTSQL.v1
{
    public static class LTSQLQueryableExtensions
    {
        private static void AddOrder(QuerySequence sequence, Expression expr, bool desc)
        {
            var query = sequence;
            if (query.Order == null)
            {
                query.Order = new OrderUnit();
                query.Order.OrderKeys = new List<KeyValuePair<Expression, bool>>(2);
            }
            query.Order.OrderKeys.Add(new KeyValuePair<Expression, bool>(expr, desc));
        }


        //初始化查询对象，以支持LINQ语法
        public static ILTSQLOrderedQueryable<T> AsLTSQL<T>(this T obj) where T : class
        {
            QuerySequence query = new QuerySequence();
            query.From = new FromUnit();
            query.Type = typeof(T);
            query.From.Source = new SimpleSequence(typeof(T));

            var ltsql = new LTSQLObject<T>();
            ltsql.Query = new QuerySequence();

            return new LTSQLObject<T>() { Query = query};
        }
        //where
        public static ILTSQLObjectQueryable<T> Where<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, bool>> expr)
        {
            QuerySequence query = src.Query;
            if (query.Where == null)
            {
                query.Where = new WhereUnit();
                query.Where.Conditions = new List<Expression>(4);
            }
            query.Where.Conditions.Add(expr);
            return src;
        }
        //order
        public static ILTSQLOrderedQueryable<T> OrderBy<T, TKey>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.Query;
            AddOrder(query, keyExpr, false);
            return new LTSQLObject<T>(query);
        }
        public static ILTSQLOrderedQueryable<T> OrderByDescending<T, TKey>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.Query;
            AddOrder(query, keyExpr, true);
            return new LTSQLObject<T>(query);
        }
        public static ILTSQLOrderedQueryable<T> ThenBy<T, TKey>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.Query;
            AddOrder(query, keyExpr, false);
            return new LTSQLObject<T>(query);
        }
        public static ILTSQLOrderedQueryable<T> ThenByDescending<T, TKey>(this ILTSQLOrderedQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.Query;
            AddOrder(query, keyExpr, true);
            return new LTSQLObject<T>(query);
        }

        //group
        public static ILTSQLObjectQueryable<IGrouping<TKey, T>> GroupBy<T,TKey>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.Query;
            query.Group.GroupKeys = keyExpr;
            return new LTSQLObject<IGrouping<TKey, T>>(query);
        }
        //select
        public static ILTSQLObjectQueryable<TResult> Select<T,TResult>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> expr)
        {
            var query = src.Query;
            if (query.Select == null)
                query.Select = new SelectUnit();

            
            query.Select.SelectKey = expr;
            query.Select.NewType = typeof(TResult);

            return new LTSQLObject<TResult>(new QuerySequence()
            {
                Type = typeof(TResult),
                From = new FromUnit() { 
                    Source = query 
                }
            });
        }
        //join
        public static ILTSQLObjectQueryable<TResult> Join<TOuter, TInner, TKey, TResult>(
            this ILTSQLObjectQueryable<TOuter> outer,
            ILTSQLObjectQueryable<TInner> inner,
            Expression<Func<TOuter, TKey>> outerKeyExpr,
            Expression<Func<TInner, TKey>> innerKeyExpr,
            Expression<Func<TOuter, TInner, TResult>> joinExpr)
        {
            FromUnit from = null;
            if (outer.Query.IsSimpleSelect())
            {
                //连续 join 时需要合并 from 子句
                from = outer.Query.From;
            }
            else
            {
                //join 子查询
                from = new FromUnit();
                from.Source = outer.Query;
            }

            //需要检验参数命名是否相同
            return new LTSQLObject<TResult>(new QuerySequence
            {
                Type = typeof(TResult),
                From = new FromJoinUnit()
                {
                    From = from,
                    Source = inner.Query,
                    Source1Key = outerKeyExpr,
                    Source2Key = innerKeyExpr,
                    JoinExpr = joinExpr
                }
            });
        }
    }




    internal class LTSQLObject<T> : ILTSQLOrderedQueryable<T>
    {
        public LTSQLObject() 
        { }
        public LTSQLObject(QuerySequence query)
        {
            this.Query = query;
        }


        public QuerySequence Query { get; set; }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public interface ILTSQLObjectQueryable
    {
        //保存查询的结构
        public QuerySequence Query { get; set; }
    }
    public interface ILTSQLObjectQueryable<T> : IEnumerable<T>, ILTSQLObjectQueryable
    { }
    public interface ILTSQLOrderedQueryable<T> : ILTSQLObjectQueryable<T>
    { }
}
