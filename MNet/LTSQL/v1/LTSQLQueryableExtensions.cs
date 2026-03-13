using MNet.LTSQL.v1.SqlQueryStructs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Linq;

namespace MNet.LTSQL.v1
{
    public static class LTSQLQueryableExtensions
    {
        private static QuerySequence TryNewTurn(this QuerySequence seq)
        {
            if (seq.Step == QueryStep.Select)
            {
                return new QuerySequence()
                {
                    Type = seq.NewType,
                    From = new FromPart()
                    {
                        Seq = seq
                    }
                };
            }

            return seq;
        }
        private static QuerySequence TryNextStep(this QuerySequence seq, QueryStep step)
        {
            if (seq.Step < step)
                seq.Step = step;
            return seq;
        }
        private static void AddOrder(ref QuerySequence sequence, Expression expr, bool desc)
        {
            sequence = sequence.TryNewTurn();
            sequence.TryNextStep(QueryStep.OrderBy);

            var query = sequence;
            query.Orders ??= new List<OrderKeyPart>();
            query.Orders.Add(new OrderKeyPart() { Key = expr, Asc = !desc });
        }



        //初始化查询对象，以支持LINQ语法
        public static ILTSQLOrderedQueryable<T> AsLTSQL<T>(this T obj) where T : class
        {
            QuerySequence query = new QuerySequence();
            query.Step = QueryStep.From;
            query.Type = typeof(T);
            query.From = new FromPart();
            query.From.Parent = null;
            query.From.Seq = new SimpleSequence(typeof(T));


            var ltsql = new LTSQLObject<T>();
            ltsql.Query = query;

            return ltsql;
        }


        public static ILTSQLObjectQueryable<T> Skip<T>(this ILTSQLObjectQueryable<T> src, int skip)
        {
            return WithSkip(src, skip) as ILTSQLObjectQueryable<T>;
        }
        public static ILTSQLObjectQueryable WithSkip(this ILTSQLObjectQueryable src, int skip)
        {
            src.Query.TryNextStep(QueryStep.Query);
            src.Query.Skip = skip;
            return src;
        }

        public static ILTSQLObjectQueryable<T> Take<T>(this ILTSQLObjectQueryable<T> src, int take)
        {
            return WithTake(src, take) as ILTSQLObjectQueryable<T>;
        }
        public static ILTSQLObjectQueryable WithTake(this ILTSQLObjectQueryable src, int take)
        {
            src.Query.TryNextStep(QueryStep.Query);
            src.Query.Take = take;
            return src;
        }

        public static ILTSQLObjectQueryable<T> Distinct<T>(this ILTSQLObjectQueryable<T> src)
        {
            return WithDistinct(src) as ILTSQLObjectQueryable<T>;
        }
        public static ILTSQLObjectQueryable WithDistinct(this ILTSQLObjectQueryable src)
        {
            src.Query.TryNextStep(QueryStep.Query);
            src.Query.Distinct = true;
            return src;
        }

        //where
        public static ILTSQLObjectQueryable<T> Where<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, bool>> expr)
        {
            QuerySequence query = src.Query.TryNewTurn();
            query.TryNextStep(QueryStep.Where);
            query.Wheres ??= new List<Expression>();
            query.Wheres.Add(expr);
            return src;
        }
        //having
        public static ILTSQLObjectQueryable<IGrouping<TKey, T>> Where<T, TKey>(this ILTSQLObjectQueryable<IGrouping<TKey, T>> src, Expression<Func<IGrouping<TKey, T>, bool>> expr)
        {
            QuerySequence query = src.Query.TryNewTurn();
            query.TryNextStep(QueryStep.Having);
            query.Havings ??= new List<Expression>();
            query.Havings.Add(expr);
            return src;
        }

        //order
        public static ILTSQLOrderedQueryable<T> OrderBy<T, TKey>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.Query;
            AddOrder(ref query, keyExpr, false);
            return new LTSQLObject<T>(query);
        }
        public static ILTSQLOrderedQueryable<T> OrderByDescending<T, TKey>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.Query;
            AddOrder(ref query, keyExpr, true);
            return new LTSQLObject<T>(query);
        }
        public static ILTSQLOrderedQueryable<T> ThenBy<T, TKey>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.Query;
            AddOrder(ref query, keyExpr, false);
            return new LTSQLObject<T>(query);
        }
        public static ILTSQLOrderedQueryable<T> ThenByDescending<T, TKey>(this ILTSQLOrderedQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.Query;
            AddOrder(ref query, keyExpr, true);
            return new LTSQLObject<T>(query);
        }

        //group
        public static ILTSQLObjectQueryable<IGrouping<TKey, T>> GroupBy<T,TKey>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.Query.TryNewTurn();
            query.TryNextStep(QueryStep.GroupBy);
            query.GroupKey = keyExpr;
            query.GroupElement = (Expression<Func<T, T>>)(p => p); //默认的分组元素为整个对象
            query.Step = QueryStep.GroupBy;
            return new LTSQLObject<IGrouping<TKey, T>>(query);
        }
        public static ILTSQLObjectQueryable<IGrouping<TKey, TElement>> GroupBy<T, TKey, TElement>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr, Expression<Func<T, TElement>> elementExpr)
        {
            var query = src.Query.TryNewTurn();
            query.TryNextStep(QueryStep.GroupBy);
            query.GroupKey = keyExpr;
            query.GroupElement = elementExpr;
            return new LTSQLObject<IGrouping<TKey, TElement>>(query);
        }

        //select
        public static ILTSQLObjectQueryable<TResult> Select<T,TResult>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> expr)
        {
            var query = src.Query.TryNewTurn();
            query.SelectKey = expr;
            query.Step = QueryStep.Select;
            query.NewType = typeof(TResult);

            return new LTSQLObject<TResult>(query);
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
            if ((int)qOuter.Step <= (int)QueryStep.Join)
            {
                //连续的join
                fromJoin.Parent = qOuter.From;
            }
            else
            {
                //直接作为子查询
                fromJoin.Parent = new FromPart(qOuter);
            }


            if ((int)qInner.Step < (int)QueryStep.Join)
            {
                //join 一张表
                fromJoin.Seq = qInner.From.Seq;
            }
            else
            {
                //join 子查询
                fromJoin.Seq = qInner;
            }

            //如果是手工方法调用，则需要检验join表达式中，参数命名是否能够推出表命名来
            fromJoin.JoinType = "LEFT JOIN";
            fromJoin.JoinKey1 = outerKeyExpr;
            fromJoin.JoinKey2 = innerKeyExpr;
            //fromJoin.JoinKeyOn = joinExpr;
            fromJoin.JoinObject = joinExpr;

            return new LTSQLObject<TResult>(new QuerySequence
            {
                From = fromJoin,
                Step = QueryStep.Join,
                Type = typeof(TResult)
            });
        }


        //占位函数，用于 linq 表达式写法，其效果等同于Take(1)函数调用
        public static T FirstOrDefault<T>(this ILTSQLObjectQueryable<T> src)
        {
            src.Take(1);
            return default(T);
        }
        //直接聚合函数
        public static ILTSQLObjectQueryable<int> WithAny<T>(this ILTSQLObjectQueryable<T> src)
        {
            return src.Select(p => 1).Take(1);
        }
        public static ILTSQLObjectQueryable<int> WithCount<T>(this ILTSQLObjectQueryable<T> src)
        {
            return from m in src.Select(p => new { flag = 1 })
                   group m by m.flag into g
                   select g.Count();
        }
        public static ILTSQLObjectQueryable<long> WithLongCount<T>(this ILTSQLObjectQueryable<T> src)
        {
            return from m in src.Select(p => new { flag = 1 })
                   group m by m.flag into g
                   select g.LongCount();
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
