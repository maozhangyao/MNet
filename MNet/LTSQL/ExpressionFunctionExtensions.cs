using System;
using System.Linq;
using System.Collections.Generic;

namespace MNet.LTSQL
{
    public static class ExpressionFunctionExtensions
    {
        //占位函数，用于 linq 表达式写法，其效果等同于Take(1)函数调用
        public static T FirstOrDefault<T>(this ILTSQLObjectQueryable<T> src)
        {
            src.Take(1);
            return default(T);
        }
        //in操作之，元组匹配
        public static bool In<T>(this T tuple, params object[] items)
        {
            return tuple.In(items);
        }
        //in操作之，元组匹配
        public static bool In<T>(this T tuple, IEnumerable<object> items)
        {
            return false;
        }   
    }

}