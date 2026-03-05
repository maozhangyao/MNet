using MNet.LTSQL.v1.SqlQueryStructs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
            query.Step = QueryStep.OrderBy;
            query.Orders ??= new List<OrderKey>();
            query.Orders.Add(new OrderKey() { Key = expr, Asc = !desc });
        }


        //初始化查询对象，以支持LINQ语法
        public static ILTSQLOrderedQueryable<T> AsLTSQL<T>(this T obj) where T : class
        {
            QuerySequence query = new QuerySequence();
            query.Step = QueryStep.From;
            query.Type = typeof(T);
            query.F = new FromPart();
            query.F.Parent = null;
            query.F.Seq = new SimpleSequence(typeof(T));


            var ltsql = new LTSQLObject<T>();
            ltsql.Query = query;

            return ltsql;
        }
        //where
        public static ILTSQLObjectQueryable<T> Where<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, bool>> expr)
        {
            QuerySequence query = src.Query;
            query.Wheres ??= new List<Expression>();
            query.Wheres.Add(expr);
            query.Step = QueryStep.Where;
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
            query.GroupKey = keyExpr;
            query.GroupElement = (Expression<Func<T, T>>)(p => p); //默认的分组元素为整个对象
            query.Step = QueryStep.GroupBy;
            return new LTSQLObject<IGrouping<TKey, T>>(query);
        }
        public static ILTSQLObjectQueryable<IGrouping<TKey, TElement>> GroupBy<T, TKey, TElement>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr, Expression<Func<T, TElement>> elementExpr)
        {
            var query = src.Query;
            query.GroupKey = keyExpr;
            query.GroupElement = elementExpr;
            query.Step = QueryStep.GroupBy;
            return new LTSQLObject<IGrouping<TKey, TElement>>(query);
        }

        //select
        public static ILTSQLObjectQueryable<TResult> Select<T,TResult>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> expr)
        {
            var query = src.Query;
            query.SelectKey = expr;
            query.Step = QueryStep.Select;
            query.NewType = typeof(TResult);

            return new LTSQLObject<TResult>(new QuerySequence()
            {
                Type = typeof(TResult),
                Step = QueryStep.From,
                F = new FromPart()
                {
                    Parent = null,
                    Seq = query
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
            QuerySequence qOuter = outer.Query;
            QuerySequence qInner = inner.Query;
            FromPart fromJoin = new FromPart();
            if ((int)qOuter.Step > (int)QueryStep.Join)
            {
                //直接作为子查询
                fromJoin.Parent = new FromPart();
                fromJoin.Parent.Seq = qOuter;
            }
            else
            {
                fromJoin.Parent = qOuter.F;
            }


            if ((int)qInner.Step > (int)QueryStep.From)
            {
                //作为子查询
                fromJoin.Seq = qInner;
            }
            else
            {
                fromJoin.Seq = qInner.F.Seq;
            }

            //如果是手工方法调用，则需要检验join表达式中，参数命名是否能够推出表命名来
            fromJoin.JoinType = "LEFT JOIN";
            fromJoin.JoinKey1 = outerKeyExpr;
            fromJoin.JoinKey2 = outerKeyExpr;
            //fromJoin.JoinKeyOn = joinExpr;
            fromJoin.JoinObject = joinExpr;

            return new LTSQLObject<TResult>(new QuerySequence
            {
                F = fromJoin,
                Step = QueryStep.Join,
                Type = typeof(TResult)
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
