using MNet.LTSQL.SqlQueryStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MNet.LTSQL
{
    /// <summary>
    /// 表达式中的方法占位符，仅用于表达式翻译中使用，实际不含任何逻辑，请勿直接使用
    /// </summary>
    public static class ExpressionFunctionExtensions
    {
        /// <summary>
        /// 将值转换为bool。在sql中 0=false,1=true
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool? ToBool(this object val)
        {
            return default;
        }
        /// <summary>
        /// 将值转换为int
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static int? ToInt(this object val)
        {
            return default;
        }
        /// <summary>
        /// 将值转换为long
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static long? ToLong(this object val)
        {
            return default;
        }
        /// <summary>
        /// 将值转换为double
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static double? ToDouble(this object val)
        {
            return default;
        }
        /// <summary>
        /// 将值转换为decimal
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static decimal? ToDecimal(this object val)
        {
            return default;
        }
        /// <summary>
        /// exists 操作符
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static bool Any<T>(this ILTSQLObjectQueryable<T> src)
        {
            return default;
        }
        /// <summary>
        /// in操作之元组匹配
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tuple"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static bool In<T>(this T tuple, params object[] items)
        {
            return false;
        }
        public static bool In<T>(this T tuple, IEnumerable<object> items)
        {
            return false;
        }
        /// <summary>
        /// 仅仅取出首行记录，用于 linq 表达式写法，其效果等同于Take(1)函数调用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static T FirstOrDefault<T>(this ILTSQLObjectQueryable<T> src)
        {
            //src.Query = src.Take(1).Query;
            return default(T);
        }

        /// <summary>
        /// 终结点聚合函数支持
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="src"></param>
        /// <param name="exprOfmax"></param>
        /// <returns></returns>
        public static TResult Max<T, TResult>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> exprOfmax)
        {
            return default;
        }
        public static TResult Min<T, TResult>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> exprOfmin)
        {
            return default;
        }
        public static TResult Sum<T, TResult>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> exprOfmax)
        {
            return default;
        }
        public static TResult Average<T, TResult>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> exprOfAverage)
        {
            return default;
        }
        public static int Count<T>(this ILTSQLObjectQueryable<T> src)
        {
            return 0;
        }
        public static int Count<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, object>> exprOfcount)
        {
            return 0;
        }
        public static long LongCount<T>(this ILTSQLObjectQueryable<T> src)
        {
            return 0;
        }
        public static long LongCount<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, object>> exprOfcount)
        {
            return 0;
        }

        /// <summary>
        /// 聚合函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="group"></param>
        /// <param name="exprOfmax"></param>
        /// <returns></returns>
        public static TResult Max<T, TKey, TResult>(this IGrouping<TKey,T> group, Expression<Func<T, TResult>> exprOfmax)
        {
            return default;
        }
        public static TResult Min<T, TKey, TResult>(this IGrouping<TKey, T> group, Expression<Func<T, TResult>> exprOfmin)
        {
            return default;
        }
        public static TResult Sum<T, TKey, TResult>(this IGrouping<TKey, T> group, Expression<Func<T, TResult>> exprOfmax)
        {
            return default;
        }
        public static TResult Average<T, TKey, TResult>(this IGrouping<TKey, T> group, Expression<Func<T, TResult>> exprOfAverage)
        {
            return default;
        }
        public static int Count<T, TKey>(this IGrouping<TKey, T> group)
        {
            return 0;
        }
        public static int Count<T, TKey>(this IGrouping<TKey, T> group, Expression<Func<T, object>> exprOfcount)
        {
            return 0;
        }
        public static long LongCount<T, TKey>(this IGrouping<TKey, T> group)
        {
            return 0;
        }
        public static long LongCount<T, TKey>(this IGrouping<TKey, T> group, Expression<Func<T, object>> exprOfcount)
        {
            return 0;
        }
    }


    internal static class InternalExpressionGenerator
    {
        /// <summary>
        /// 在ExpressionFunctionExtensions类型上找到IGrouping<,>的扩展方法
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private static MethodInfo GetIGroupingExtMethodOnExpressionFunctionExtensions(string method)
        {
            return GetIGroupingExtMethodsOnExpressionFunctionExtensions(method).First();
        }
        private static IEnumerable<MethodInfo> GetIGroupingExtMethodsOnExpressionFunctionExtensions(string method)
        {
            return typeof(ExpressionFunctionExtensions)
                .GetMethods().Where(p => p.Name == method && p.IsGenericMethod)
                .Where(p =>
                {
                    //注意 ILTSQLObjectQueryable<T> 和 ILTSQLObjectQueryable<> 是不同的两个类型
                    //前者已经是属于封闭的泛型类型，只不过封闭的类型是一个模板类型T；而后者是实实在在的开放的泛型类型。
                    ParameterInfo[] arr = p.GetParameters();
                    //参数类型是一个已经被模板类型T封闭的泛型，需要获取泛型定义
                    return arr[0].ParameterType.GetGenericTypeDefinition() == typeof(IGrouping<,>);
                }).ToArray();
        }

        /// <summary>
        /// 在IGrouping<,>对象上构造对分组方法调用的lambda表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="genericMethod"></param>
        /// <param name="exprOfagg"></param>
        /// <returns></returns>
        private static Expression<Func<IGrouping<TKey, T>, TResult>> MakeGroupMethodCallExpressionOnIGrouping<T, TKey, TResult>(MethodInfo mthd, Expression? exprOfagg = null)
        {
            //MethodInfo mthd = genericMethod.MakeGenericMethod(typeof(T), typeof(TKey), typeof(TResult));
            ParameterExpression parameter = Expression.Parameter(typeof(IGrouping<TKey, T>), "g");

            if (exprOfagg == null)
            {
                return Expression.Lambda<Func<IGrouping<TKey, T>, TResult>>(
                        Expression.Call(null, mthd, parameter),
                        parameter
                    );
            }

            return Expression.Lambda<Func<IGrouping<TKey, T>, TResult>>(
                    Expression.Call(null, mthd, parameter, exprOfagg),
                    parameter
                );
        }
        


        /// <summary>
        /// 动态的生成分组函数调用(min,max,sum,Average)
        /// </summary>
        /// <param name="groupMethod">分组函数之一：min,max,sum,Average</param>
        /// <param name="srcObjectQueryable">未做分组操作的的查询表达式</param>
        /// <param name="exprOfgroup">需要传递给分组函数的表达式参数</param>
        /// <param name="groupEle">分组元素类型</param>
        /// <param name="groupResult">分组函数的结果类型</param>
        /// <returns>返回分组后的查询表达式</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static object DynamicInvokeGroupMethod(string groupMethod, object srcObjectQueryable, Expression exprOfgroup, Type groupEle, Type groupResult)
        {
            if (string.IsNullOrEmpty(groupMethod))
                throw new ArgumentNullException(nameof(groupMethod));
            if (exprOfgroup == null)
                throw new ArgumentNullException(nameof(exprOfgroup));
            if (srcObjectQueryable == null)
                throw new ArgumentNullException(nameof(srcObjectQueryable));
            if(groupEle == null)
                throw new ArgumentNullException(nameof(groupEle));
            if(groupResult == null)
                throw new ArgumentNullException(nameof(groupResult));

            //找到InternalExpressionGenerator类中的 min,max,sum,average 分组函数并动态调用
            MethodInfo mthd = typeof(InternalExpressionGenerator).GetMethod(groupMethod);
            if (mthd == null)
                throw new Exception($"非法分组函数，{nameof(groupMethod)}:{groupMethod}");

            return mthd.MakeGenericMethod(groupEle, groupResult)
                        .Invoke(null, new [] { srcObjectQueryable, exprOfgroup });
        }
        /// <summary>
        /// 动态的生成分组函数调用(count, longCount)
        /// </summary>
        /// <param name="countMethod"></param>
        /// <param name="srcObjectQueryable"></param>
        /// <param name="exprOfgroup"></param>
        /// <param name="groupEle"></param>
        /// <param name="groupResult"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static object DynamicInvokeCountMethod(string countMethod, object srcObjectQueryable, Expression exprOfgroup, Type groupEle)
        {
            if (string.IsNullOrEmpty(countMethod))
                throw new ArgumentNullException(nameof(countMethod));
            if (srcObjectQueryable == null)
                throw new ArgumentNullException(nameof(srcObjectQueryable));
            if (groupEle == null)
                throw new ArgumentNullException(nameof(groupEle));
            
            //找到InternalExpressionGenerator类中的 count,longCount 分组函数并动态调用
            MethodInfo mthd = typeof(InternalExpressionGenerator).GetMethod(countMethod);
            if (mthd == null)
                throw new Exception($"非法分组函数，{nameof(countMethod)}:{countMethod}");

            return mthd.MakeGenericMethod(groupEle)
                        .Invoke(null, new[] { srcObjectQueryable, exprOfgroup });
        }
        
        
        public static ILTSQLObjectQueryable<TResult> Max<T, TResult>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> exprOfmax)
        {
            MethodInfo methMax = GetIGroupingExtMethodOnExpressionFunctionExtensions(nameof(ExpressionFunctionExtensions.Max))
                .MakeGenericMethod(typeof(T), typeof(T), typeof(TResult));

            //Max(g, exprOfmax);
            Expression<Func<IGrouping<T, T>, TResult>> aggOfMax = MakeGroupMethodCallExpressionOnIGrouping<T, T, TResult>(methMax, exprOfmax);
            return src.AsGroup().Select(aggOfMax);
        }
        public static ILTSQLObjectQueryable<TResult> Min<T, TResult>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> exprOfmin)
        {
            MethodInfo methMin = GetIGroupingExtMethodOnExpressionFunctionExtensions(nameof(ExpressionFunctionExtensions.Min))
                .MakeGenericMethod(typeof(T), typeof(T), typeof(TResult));

            //Min(g, exprOfmin);
            Expression<Func<IGrouping<T, T>, TResult>> aggOfMin = MakeGroupMethodCallExpressionOnIGrouping<T, T, TResult>(methMin, exprOfmin);
            return src.AsGroup().Select(aggOfMin);
        }
        public static ILTSQLObjectQueryable<TResult> Sum<T, TResult>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> exprOfmax)
        {

            MethodInfo methSum = GetIGroupingExtMethodOnExpressionFunctionExtensions(nameof(ExpressionFunctionExtensions.Sum))
                .MakeGenericMethod(typeof(T), typeof(T), typeof(TResult));

            //Sum(g, exprOfsum);
            Expression<Func<IGrouping<T, T>, TResult>> aggOfSum = MakeGroupMethodCallExpressionOnIGrouping<T, T, TResult>(methSum, exprOfmax);
            return src.AsGroup().Select(aggOfSum);
        }
        public static ILTSQLObjectQueryable<TResult> Average<T, TResult>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> exprOfAverage)
        {
            MethodInfo methAverage = GetIGroupingExtMethodOnExpressionFunctionExtensions(nameof(ExpressionFunctionExtensions.Average))
                .MakeGenericMethod(typeof(T), typeof(T), typeof(TResult));

            //Average(g, exprOfAverage);
            Expression<Func<IGrouping<T, T>, TResult>> aggOfAverage = MakeGroupMethodCallExpressionOnIGrouping<T, T, TResult>(methAverage, exprOfAverage);
            return src.AsGroup().Select(aggOfAverage);
        }
        public static ILTSQLObjectQueryable<int> Count<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, object>>? exprOfcount)
        {
            MethodInfo methCount = GetIGroupingExtMethodsOnExpressionFunctionExtensions(nameof(ExpressionFunctionExtensions.Count))
                .Where(p => p.GetParameters().Length == (exprOfcount == null ? 1 : 2)).First()
                .MakeGenericMethod(typeof(T), typeof(T));

            //Count(g, exprOfCount);
            Expression<Func<IGrouping<T, T>, int>> aggOfCount = MakeGroupMethodCallExpressionOnIGrouping<T, T, int>(methCount, exprOfcount);
            return src.AsGroup().Select(aggOfCount);
        }
        public static ILTSQLObjectQueryable<long> LongCount<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, object>>? exprOfcount)
        {
            MethodInfo methCount = GetIGroupingExtMethodsOnExpressionFunctionExtensions(nameof(ExpressionFunctionExtensions.LongCount))
                 .Where(p => p.GetParameters().Length == (exprOfcount == null ? 1 : 2)).First()
                 .MakeGenericMethod(typeof(T), typeof(T));

            //LongCount(g, exprOfCount);
            Expression<Func<IGrouping<T, T>, long>> aggOfCount = MakeGroupMethodCallExpressionOnIGrouping<T, T, long> (methCount, exprOfcount);
            return src.AsGroup().Select(aggOfCount);
        }
    }
}