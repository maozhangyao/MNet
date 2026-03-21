using MNet.LTSQL.v1.SqlQueryStructs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
        private static SqlQueryPart SetNextStep(this SqlQueryPart query, QueryStepSeq step, bool equals = true)
        {
            if (query.Step >= step)
            {
                if (equals && query.Step == step)
                    return query;

                return new SqlQueryPart()
                {
                    Step = step,
                    MappingType = query.NewType,
                    From1 = query
                };
            }
            query.Step = step;
            return query;
        }
        private static void AddOrder(ref SqlQueryPart sequence, Expression expr, bool desc)
        {
            sequence = sequence.SetNextStep(QueryStepSeq.OrderBy);
            sequence.Orders ??= new List<OrderKeyPart>();
            sequence.Orders.Add(new OrderKeyPart() { Key = expr, Asc = !desc });
        }
        //将序列直接转换为分组模式(不带group by 子句)
        private static ILTSQLObjectQueryable<IGrouping<TKey, T>> AsGroup<TKey,T>(this ILTSQLObjectQueryable<T> src)
        {
            src = src.AsLTSQL();
            src.Query = src.Query.SetNextStep(QueryStepSeq.GroupBy);
            src.Query.GroupFlag = true;
            src.Query.GroupElement = (Expression<Func<T, T>>)(p => p);
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
            query.Step = QueryStepSeq.From;
            query.MappingType = typeof(T);
            query.From1 = tablePart;

            var ltsql = new LTSQLObject<T>();
            ltsql.Query = query;

            return ltsql;
        }
        //指定from为子查询形式的数据源，开启新的外层查询
        public static ILTSQLOrderedQueryable<T> AsLTSQL<T>(this ILTSQLObjectQueryable<T> frm)
        {
            SqlQueryPart query = new SqlQueryPart();
            query.Step = QueryStepSeq.From;
            query.MappingType = typeof(T);
            query.From1 = frm.Query.CopyNew();

            return new LTSQLObject<T>(query);
        }


        public static ILTSQLObjectQueryable<T> Skip<T>(this ILTSQLObjectQueryable<T> src, int skip)
        {
            return WithSkip(src, skip) as ILTSQLObjectQueryable<T>;
        }
        public static ILTSQLObjectQueryable WithSkip(this ILTSQLObjectQueryable src, int skip)
        {
            src.Query = src.Query.CopyNew() as SqlQueryPart;
            src.Query.SetNextStep(QueryStepSeq.Page);
            src.Query.Skip = skip;
            return src;
        }

        public static ILTSQLObjectQueryable<T> Take<T>(this ILTSQLObjectQueryable<T> src, int take)
        {
            return WithTake(src, take) as ILTSQLObjectQueryable<T>;
        }
        public static ILTSQLObjectQueryable WithTake(this ILTSQLObjectQueryable src, int take)
        {
            src.Query = src.Query.CopyNew() as SqlQueryPart;
            src.Query.SetNextStep(QueryStepSeq.Page);
            src.Query.Take = take;
            return src;
        }

        public static ILTSQLObjectQueryable<T> Distinct<T>(this ILTSQLObjectQueryable<T> src)
        {
            return WithDistinct(src) as ILTSQLObjectQueryable<T>;
        }
        public static ILTSQLObjectQueryable WithDistinct(this ILTSQLObjectQueryable src)
        {
            src.Query = src.Query.CopyNew() as SqlQueryPart;
            src.Query.SetNextStep(QueryStepSeq.Query);
            src.Query.Distinct = true;
            return src;
        }

        //where
        public static ILTSQLObjectQueryable<T> Where<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, bool>> expr)
        {
            src.Query = src.Query.CopyNew() as SqlQueryPart;

            SqlQueryPart query = src.Query.SetNextStep(QueryStepSeq.Where);
            query.Wheres ??= new List<Expression>();
            query.Wheres.Add(expr);

            return new LTSQLObject<T>(query);
        }
        //having
        public static ILTSQLObjectQueryable<IGrouping<TKey, T>> Where<T, TKey>(this ILTSQLObjectQueryable<IGrouping<TKey, T>> src, Expression<Func<IGrouping<TKey, T>, bool>> expr)
        {
            src.Query = src.Query.CopyNew() as SqlQueryPart;

            SqlQueryPart query = src.Query.SetNextStep(QueryStepSeq.Having);
            query.Havings ??= new List<Expression>();
            query.Havings.Add(expr);
            return new LTSQLObject<IGrouping<TKey, T>>(query);
        }

        //order
        public static ILTSQLOrderedQueryable<T> OrderBy<T, TKey>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.Query.CopyNew() as SqlQueryPart;
            AddOrder(ref query, keyExpr, false);

            return new LTSQLObject<T>(query);
        }
        public static ILTSQLOrderedQueryable<T> OrderByDescending<T, TKey>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.Query.CopyNew() as SqlQueryPart;
            AddOrder(ref query, keyExpr, true);

            return new LTSQLObject<T>(query);
        }
        public static ILTSQLOrderedQueryable<T> ThenBy<T, TKey>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.Query.CopyNew() as SqlQueryPart;
            AddOrder(ref query, keyExpr, false);
;
            return new LTSQLObject<T>(query);
        }
        public static ILTSQLOrderedQueryable<T> ThenByDescending<T, TKey>(this ILTSQLOrderedQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.Query.CopyNew() as SqlQueryPart;
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
            var query = (src.Query.CopyNew() as SqlQueryPart)
                .SetNextStep(QueryStepSeq.GroupBy, false);
            
            query.GroupFlag = true;
            query.GroupKey = keyExpr;
            query.GroupElement = elementExpr;

            return new LTSQLObject<IGrouping<TKey, TElement>>(query);
        }

        //select
        public static ILTSQLObjectQueryable<TResult> Select<T, TResult>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> expr)
        {
            var query = (src.Query.CopyNew() as SqlQueryPart)
                .SetNextStep(QueryStepSeq.Select, false);
            
            query.SelectKey = expr;
            query.NewType = typeof(TResult);

            return new LTSQLObject<TResult>(query);
        }
        //join
        public static ILTSQLObjectQueryable<TResult> Join<TOuter, TInner, TKey, TResult>(this ILTSQLObjectQueryable<TOuter> outer
            ,ILTSQLObjectQueryable<TInner> inner
            ,Expression<Func<TOuter, TKey>> outerKeyExpr
            ,Expression<Func<TInner, TKey>> innerKeyExpr
            ,Expression<Func<TOuter, TInner, TResult>> joinExpr)
        {
            SqlQueryPart qOuter = outer.Query.CopyNew() as SqlQueryPart;
            SqlQueryPart qInner = inner.Query.CopyNew() as SqlQueryPart;
            JoinPart joinPart = new JoinPart();

            //如果是手工方法调用，则需要检验join表达式中，参数命名是否能够推出表命名来
            joinPart.JoinType = "LEFT JOIN";
            joinPart.JoinKey1 = outerKeyExpr;
            joinPart.JoinKey2 = innerKeyExpr;
            joinPart.JoinObject = joinExpr;
            joinPart.MappingType = typeof(TResult);

            //判断匿名类所在的属性
            if (qOuter.Step == QueryStepSeq.Join)
            {
                Type anymouseType = typeof(TResult);
                foreach (PropertyInfo prop in anymouseType.GetProperties())
                {
                    if (prop.PropertyType == typeof(TOuter))
                        joinPart.JoinKey1Prop = prop.Name;
                }
            }


            if ((int)qOuter.Step <= (int)QueryStepSeq.Join)
            {
                //连续的join
                joinPart.MainQuery = qOuter.From1;
            }
            else
            {
                //直接作为子查询
                joinPart.MainQuery = qOuter;
            }


            if ((int)qInner.Step < (int)QueryStepSeq.Join)
            {
                //join 一张表
                joinPart.JoinQuery = qInner.From1;
            }
            else
            {
                //join 子查询
                joinPart.JoinQuery = qInner;
            }

            return new LTSQLObject<TResult>(new SqlQueryPart
            {
                From1 = joinPart,
                Step = QueryStepSeq.Join,
                MappingType = typeof(TResult)
            });
        }

        // select Many
        public static ILTSQLObjectQueryable<TResult> SelectMany<TSource, TCollection, TResult>(this ILTSQLObjectQueryable<TSource> source
            , Expression<Func<TSource, ILTSQLObjectQueryable<TCollection>>> collectionSelector
            , Expression<Func<TSource, TCollection, TResult>> resultSelector)
        {
            ILTSQLObjectQueryable<TCollection> inner = collectionSelector.Compile().Invoke(default(TSource));

            SqlQueryPart qOuter = source.Query.CopyNew() as SqlQueryPart;
            SqlQueryPart qInner = inner.Query.CopyNew() as SqlQueryPart;

            JoinPart join = new JoinPart();
            join.JoinObject = resultSelector;

            Type anymouseType = typeof(TResult);
            //确定主表属性
            if (typeof(TSource) == typeof(TCollection))
            {
                join.JoinKey1Prop = anymouseType.GetProperties()[0].Name;
            }
            else
            {
                foreach (PropertyInfo prop in anymouseType.GetProperties())
                {
                    if (prop.PropertyType == typeof(TSource))
                        join.JoinKey1Prop = prop.Name;
                }
            }

            join.MainQuery = qOuter.From1;
            //非连续join
            if (qOuter.Step > QueryStepSeq.Join)
                join.MainQuery = qOuter;

            join.JoinQuery = qInner;
            if (qInner.Step < QueryStepSeq.Join)
                join.JoinQuery = qInner.From1;

            SqlQueryPart query = new SqlQueryPart();
            query.From1 = join;
            query.Step = QueryStepSeq.Join;
            query.MappingType = typeof(TResult);
            return new LTSQLObject<TResult>(query);
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
