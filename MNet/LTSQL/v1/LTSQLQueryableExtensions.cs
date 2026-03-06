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
        //where
        public static ILTSQLObjectQueryable<T> Where<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, bool>> expr)
        {
            QuerySequence query = src.Query.TryNewTurn();
            query.TryNextStep(QueryStep.Where);
            query.Wheres ??= new List<Expression>();
            query.Wheres.Add(expr);
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
            fromJoin.JoinKey2 = outerKeyExpr;
            //fromJoin.JoinKeyOn = joinExpr;
            fromJoin.JoinObject = joinExpr;

            return new LTSQLObject<TResult>(new QuerySequence
            {
                From = fromJoin,
                Step = QueryStep.Join,
                Type = typeof(TResult)
            });
        }


        public static ILTSQLObjectQueryable<T> Skip<T>(this ILTSQLObjectQueryable<T> src, int skip)
        {
            src.Query.TryNextStep(QueryStep.Query);
            src.Query.Skip = skip;
            return src;
        }
        public static ILTSQLObjectQueryable<T> Take<T>(this ILTSQLObjectQueryable<T> src, int take)
        {
            src.Query.TryNextStep(QueryStep.Query);
            src.Query.Take = take;
            return src;
        }
        public static ILTSQLObjectQueryable<T> Distinct<T>(this ILTSQLObjectQueryable<T> src)
        {
            src.Query.TryNextStep(QueryStep.Query);
            src.Query.Distinct = true;
            return src;
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
