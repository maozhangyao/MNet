using System;
using System.Linq;
using System.Collections.Generic;

namespace MNet.LTSQL
{
    /// <summary>
    /// 表达式中的方法占位符，仅用于表达式翻译中使用，实际不含任何逻辑，请勿直接使用
    /// </summary>
    public static class ExpressionFunctionExtensions
    {
        public static int? ToInt(this object val)
        {
            return default;
        }
        public static long? ToLong(this object val)
        {
            return default;
        }
        public static double? ToDouble(this object val)
        {
            return default;
        }
        public static decimal? ToDecimal(this object val)
        {
            return default;
        }
        public static bool? ToBoolean(this object val)
        {
            return default;
        }

        //占位函数，用于 linq 表达式写法，其效果等同于Take(1)函数调用
        public static T FirstOrDefault<T>(this ILTSQLObjectQueryable<T> src)
        {
            //src.Query = src.Take(1).Query;
            return default(T);
        }
        //in操作之，元组匹配
        public static bool In<T>(this T tuple, params object[] items)
        {
            return false;
        }
        //in操作之，元组匹配
        public static bool In<T>(this T tuple, IEnumerable<object> items)
        {
            return false;
        }   
    }

}