using MNet.LTSQL.v1.SqlQueryStructs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace MNet.LTSQL.v1
{
    public static class LTSQLQueryableExtensions
    {
        private static SqlQueryPart TryNewTurn(this SqlQueryPart seq)
        {
            if (seq.Step == QueryStep.Select)
            {
                return new SqlQueryPart()
                {
                    MappingType = seq.NewType,
                    From = new FromPart()
                    {
                        Seq = seq
                    }
                };
            }

            return seq;
        }
        private static SqlQueryPart TryNextStep(this SqlQueryPart seq, QueryStep step)
        {
            if (seq.Step < step)
                seq.Step = step;
            return seq;
        }
        private static void AddOrder(ref SqlQueryPart sequence, Expression expr, bool desc)
        {
            sequence = sequence.TryNewTurn();
            sequence.TryNextStep(QueryStep.OrderBy);

            var query = sequence;
            query.Orders ??= new List<OrderKeyPart>();
            query.Orders.Add(new OrderKeyPart() { Key = expr, Asc = !desc });
        }
        //将序列直接转换为分组模式(不带group by 子句)
        private static ILTSQLObjectQueryable<IGrouping<TKey, T>> AsGroup<TKey,T>(this ILTSQLObjectQueryable<T> src)
        {
            src = src.AsLTSQL();
            src.Query.GroupFlag = true;
            src.Query.GroupElement = (Expression<Func<T, T>>)(p => p);
            src.Query = src.Query.TryNextStep(QueryStep.GroupBy);
            return new LTSQLObject<IGrouping<TKey, T>>(src.Query);
        }


        //初始化查询对象，以支持LINQ语法
        public static ILTSQLOrderedQueryable<T> AsLTSQL<T>(this T obj) where T : class
        {
            return AsLTSQL<T>(obj, null);
        }
        //指定表名
        public static ILTSQLOrderedQueryable<T> AsLTSQL<T>(this T obj, string tableName) where T : class
        {
            TablePart tablePart = new TablePart(typeof(T));
            tablePart.TableName = tableName;

            SqlQueryPart query = new SqlQueryPart();
            query.Step = QueryStep.From;
            query.MappingType = typeof(T);
            query.From = new FromPart();
            query.From.Parent = null;
            query.From.Seq = tablePart;


            var ltsql = new LTSQLObject<T>();
            ltsql.Query = query;

            return ltsql;
        }
        //指定from为子查询形式的数据源，开启新的外层查询
        public static ILTSQLOrderedQueryable<T> AsLTSQL<T>(this ILTSQLObjectQueryable<T> frm)
        {
            SqlQueryPart query = new SqlQueryPart();
            query.Step = QueryStep.From;
            query.MappingType = typeof(T);
            query.From = new FromPart();
            query.From.Parent = null;
            query.From.Seq = frm.Query;

            return new LTSQLObject<T>(query);
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
            SqlQueryPart query = src.Query.TryNewTurn();
            query.TryNextStep(QueryStep.Where);
            query.Wheres ??= new List<Expression>();
            query.Wheres.Add(expr);

            src.Query = query;
            return new LTSQLObject<T>(query);
        }
        //having
        public static ILTSQLObjectQueryable<IGrouping<TKey, T>> Where<T, TKey>(this ILTSQLObjectQueryable<IGrouping<TKey, T>> src, Expression<Func<IGrouping<TKey, T>, bool>> expr)
        {
            SqlQueryPart query = src.Query.TryNewTurn();
            query.TryNextStep(QueryStep.Having);
            query.Havings ??= new List<Expression>();
            query.Havings.Add(expr);
            return new LTSQLObject<IGrouping<TKey, T>>(query);
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
;
            return new LTSQLObject<T>(query);
        }
        public static ILTSQLOrderedQueryable<T> ThenByDescending<T, TKey>(this ILTSQLOrderedQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.Query;
            AddOrder(ref query, keyExpr, true);

            src.Query = query;
            return new LTSQLObject<T>(query);
        }

        //group
        public static ILTSQLObjectQueryable<IGrouping<TKey, T>> GroupBy<T,TKey>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            //默认的分组元素为整个对象
            return GroupBy(src, keyExpr, p => p);
        }
        public static ILTSQLObjectQueryable<IGrouping<TKey, TElement>> GroupBy<T, TKey, TElement>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr, Expression<Func<T, TElement>> elementExpr)
        {
            var query = src.Query.TryNewTurn();
            query.TryNextStep(QueryStep.GroupBy);
            query.GroupFlag = true;
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
            SqlQueryPart qOuter = outer.Query;
            SqlQueryPart qInner = inner.Query;
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

            return new LTSQLObject<TResult>(new SqlQueryPart
            {
                From = fromJoin,
                Step = QueryStep.Join,
                MappingType = typeof(TResult)
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
            return src.AsLTSQL().Where(p => src.Any()).Take(1).Select(p => 1);
        }
        public static ILTSQLObjectQueryable<int> WithCount<T>(this ILTSQLObjectQueryable<T> src)
        {
            return src.AsGroup<int, T>().Select(g => g.Count());
        }
        public static ILTSQLObjectQueryable<int> WithCount<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, bool>> selector)
        {
            return WithGroup<T, bool, int>(nameof(Enumerable.Count), src, selector);
        }
        public static ILTSQLObjectQueryable<long> WithLongCount<T>(this ILTSQLObjectQueryable<T> src)
        {
            return src.AsGroup<int, T>().Select(g => g.LongCount());
        }
        public static ILTSQLObjectQueryable<long> WithLongCount<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, bool>> selector)
        {
            return WithGroup<T, bool, long>(nameof(Enumerable.LongCount), src, selector);
        }


        public static ILTSQLObjectQueryable<int> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T,int>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }
        public static ILTSQLObjectQueryable<int?> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, int?>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }
        public static ILTSQLObjectQueryable<long> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, long>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }
        public static ILTSQLObjectQueryable<long?> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, long?>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }
        public static ILTSQLObjectQueryable<float> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, float>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }
        public static ILTSQLObjectQueryable<float?> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, float?>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }
        public static ILTSQLObjectQueryable<double> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, double>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }
        public static ILTSQLObjectQueryable<double?> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, double?>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }
        public static ILTSQLObjectQueryable<decimal> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, decimal>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }
        public static ILTSQLObjectQueryable<decimal?> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, decimal?>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }


        public static ILTSQLObjectQueryable<int> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, int>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }
        public static ILTSQLObjectQueryable<int?> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, int?>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }
        public static ILTSQLObjectQueryable<long> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, long>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }
        public static ILTSQLObjectQueryable<long?> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, long?>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }
        public static ILTSQLObjectQueryable<float> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, float>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }
        public static ILTSQLObjectQueryable<float?> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, float?>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }
        public static ILTSQLObjectQueryable<double> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, double>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }
        public static ILTSQLObjectQueryable<double?> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, double?>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }
        public static ILTSQLObjectQueryable<decimal> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, decimal>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }
        public static ILTSQLObjectQueryable<decimal?> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, decimal?>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }


        public static ILTSQLObjectQueryable<int> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, int>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }
        public static ILTSQLObjectQueryable<int?> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, int?>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }
        public static ILTSQLObjectQueryable<long> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, long>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }
        public static ILTSQLObjectQueryable<long?> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, long?>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }
        public static ILTSQLObjectQueryable<float> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, float>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }
        public static ILTSQLObjectQueryable<float?> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, float?>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }
        public static ILTSQLObjectQueryable<double> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, double>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }
        public static ILTSQLObjectQueryable<double?> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, double?>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }
        public static ILTSQLObjectQueryable<decimal> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, decimal>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }
        public static ILTSQLObjectQueryable<decimal?> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, decimal?>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }


        public static ILTSQLObjectQueryable<double> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, int>> selector)
        {
            return WithGroup<T,int, double>(nameof(Enumerable.Average), src, selector);
        }
        public static ILTSQLObjectQueryable<double?> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, int?>> selector)
        {
            return WithGroup<T, int?, double?>(nameof(Enumerable.Average), src, selector);
        }
        public static ILTSQLObjectQueryable<double> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, long>> selector)
        {
            return WithGroup<T, long, double>(nameof(Enumerable.Average), src, selector);
        }
        public static ILTSQLObjectQueryable<double?> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, long?>> selector)
        {
            return WithGroup<T, long?, double?>(nameof(Enumerable.Average), src, selector);
        }
        public static ILTSQLObjectQueryable<float> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, float>> selector)
        {
            return WithGroup(nameof(Enumerable.Average), src, selector);
        }
        public static ILTSQLObjectQueryable<float?> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, float?>> selector)
        {
            return WithGroup(nameof(Enumerable.Average), src, selector);
        }
        public static ILTSQLObjectQueryable<double> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, double>> selector)
        {
            return WithGroup(nameof(Enumerable.Average), src, selector);
        }
        public static ILTSQLObjectQueryable<double?> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, double?>> selector)
        {
            return WithGroup(nameof(Enumerable.Average), src, selector);
        }
        public static ILTSQLObjectQueryable<decimal> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, decimal>> selector)
        {
            return WithGroup(nameof(Enumerable.Average), src, selector);
        }
        public static ILTSQLObjectQueryable<decimal?> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, decimal?>> selector)
        {
            return WithGroup(nameof(Enumerable.Average), src, selector);
        }



        private static ILTSQLObjectQueryable<TResult> WithGroup<T, TResult>(string groupMethodName, ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> exprOfSum)
        {
            MethodInfo m = GetEnumerableGroupMethod(groupMethodName, typeof(TResult)).MakeGenericMethod(typeof(T));
            Expression<Func<IGrouping<int, T>, TResult>> expr = BuildGroupMethodExpress<T, TResult, int>(m, exprOfSum);

            return src.AsGroup<int, T>().Select(expr);
        }
        private static ILTSQLObjectQueryable<TResult> WithGroup<T, TValue, TResult>(string groupMethodName, ILTSQLObjectQueryable<T> src, Expression<Func<T, TValue>> exprOfSum)
        {
            MethodInfo m = GetEnumerableGroupMethod(groupMethodName, typeof(TResult)).MakeGenericMethod(typeof(T));
            Expression<Func<IGrouping<int, T>, TResult>> expr = BuildGroupMethodExpress<T, TValue, TResult, int>(m, exprOfSum);

            return src.AsGroup<int, T>().Select(expr);
        }
        private static MethodInfo GetEnumerableGroupMethod(string methodName, Type methodReturnType)
        {
            MethodInfo m = typeof(Enumerable).GetMethods()
                .Where(p => p.Name == methodName && p.IsGenericMethod && p.GetGenericArguments().Length == 1 && p.ReturnType == methodReturnType)
                .Where(p => p.GetParameters().Length == 2 && p.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .First();
            return m;
        }
        private static Expression<Func<IGrouping<TGroupKey, T>, TResult>> BuildGroupMethodExpress<T, TResult,TGroupKey>(MethodInfo groupMethod, Expression<Func<T, TResult>> exprOfGroup)
        {
            ParameterExpression p = Expression.Parameter(typeof(IGrouping<TGroupKey, T>));
            Expression<Func<IGrouping<TGroupKey, T>, TResult>> expr = Expression.Lambda<Func<IGrouping<TGroupKey, T>, TResult>>(
              Expression.Call(null, groupMethod, new Expression[] { p, exprOfGroup }),
              new[] { p }
               );
            return expr;
        }
        private static Expression<Func<IGrouping<TGroupKey, T>, TResult>> BuildGroupMethodExpress<T, TValue, TResult, TGroupKey>(MethodInfo groupMethod, Expression<Func<T, TValue>> exprOfGroup)
        {
            ParameterExpression p = Expression.Parameter(typeof(IGrouping<TGroupKey, T>));
            Expression<Func<IGrouping<TGroupKey, T>, TResult>> expr = Expression.Lambda<Func<IGrouping<TGroupKey, T>, TResult>>(
              Expression.Call(null, groupMethod, new Expression[] { p, exprOfGroup }),
              new[] { p }
               );
            return expr;
        }
    }
}
