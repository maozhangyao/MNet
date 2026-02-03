using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace MNet.SqlExpression
{
    public static class DbSetExtensions
    {
        private static DbPipe<T> GetDbSet<T>(IDbSet<T> src) 
        {
            var set = src as DbPipe<T>;
            if (set == null)
                throw new Exception($"无效参数{nameof(src)}");
            return set;
        }
        private static IDbSet<T> AddOrder<T, TKey>(IDbSet<T> src, Expression<Func<T, TKey>> keyExpre, bool isDesc) 
        {
            DbPipe<T> set = GetDbSet(src);
            if (keyExpre == null)
                throw new Exception($"空条件表达式：{nameof(keyExpre)}");

            set.AddOrder(keyExpre, isDesc);
            return set;
        }


        public static IDbSet<T> AsDbSet<T>() 
        {
            DbSet<T, T> head = new DbSet<T, T>();
            return head.Select(p => p);//初始的from，即自身
        }
        public static IDbSet<T> AsDbSet<T>(this T entity) 
        {
            return AsDbSet<T>();
        }
        public static bool IsDbSet(object obj)
        {
            if (obj == null)
                return false;

            Type t1 = typeof(IDbSet<>);
            Type t = obj.GetType().GetInterface(t1.Name);
            return t != null && t.GetGenericTypeDefinition() == t1;
        }
        internal static DbSetStrcut GetDbSetStruct(object obj)
        {
            if (IsDbSet(obj))
                return obj.GetType().GetProperty(nameof(DbPipe<int>.DbSet)).GetValue(obj) as DbSetStrcut;

            return null;
        }


        public static IDbSet<T> Where<T>(this IDbSet<T> src, Expression<Func<T, bool>> expr)
        {
            Console.WriteLine("WHERE =========================================================");
            Console.WriteLine(expr);
            //DbPipe<T> set = GetDbSet(src);
            //if (expr == null)
            //    throw new Exception($"空条件表达式：{nameof(expr)}");

            //set.AddWhere(expr);
            return src;
        }
        public static IDbSet<T> OrderBy<T, TKey>(this IDbSet<T> src, Expression<Func<T, TKey>> expr) 
        {
            Console.WriteLine("OrderAsc =========================================================");
            Console.WriteLine(expr);
            return null;
            return AddOrder(src, expr, false);
        }
        public static IDbSet<T> ThenBy<T, TKey>(this IDbSet<T> src, Expression<Func<T, TKey>> expr) 
        {
            Console.WriteLine("ThenOrderAsc =========================================================");
            Console.WriteLine(expr);
            return null;
            return AddOrder(src, expr, false);
        }
        public static IDbSet<T> OrderByDescending<T, TKey>(this IDbSet<T> src, Expression<Func<T, TKey>> expr) 
        {
            Console.WriteLine("OrderDesc =========================================================");
            Console.WriteLine(expr);
            return null;
            return AddOrder(src, expr, true);
        }
        public static IDbSet<T> ThenByDescending<T, TKey>(this IDbSet<T> src, Expression<Func<T, TKey>> expr) 
        {
            Console.WriteLine("ThenOrderDesc =========================================================");
            Console.WriteLine(expr);
            return null;
            return AddOrder(src, expr, true);
        }
        public static IDbSet<TResult> Select<T, TResult>(this IDbSet<T> src, Expression<Func<T, TResult>> expr)
        {
            Console.WriteLine("SELECT =========================================================");
            Console.WriteLine(expr);
            return null;
            if (src == null)
                throw new Exception($"Select {nameof(src)}参数不能为空。");

            DbPipe<T> db = GetDbSet(src);
            db.AddSelect(expr);

            DbSet<T, TResult> set = new DbSet<T, TResult>(db);
            return set;
        }
        public static IDbSet<IGrouping<TKey, T>> GroupBy<TKey, T>(this IDbSet<T> src, Expression<Func<T, TKey>> expr)
        {
            Console.WriteLine("GROUPBY =========================================================");
            Console.WriteLine(expr);
            return null;
        }
        public static IDbSet<TResult> Join<TOuter, TInner, TKey, TResult>(this IDbSet<TOuter> outer, IDbSet<TInner> inner, Expression<Func<TOuter, TKey>> outerKey, Expression<Func<TInner, TKey>> innerKey, Expression<Func<TOuter, TInner, TResult>> expr)
        {
            Console.WriteLine("JOIN =========================================================");
            Console.WriteLine($"outerKey:{outerKey}");
            Console.WriteLine($"innerKey:{innerKey}");
            Console.WriteLine($"expr:{expr}");
            return null;
        }


        public static T First<T>(this IDbSet<T> src)
        {
            DbPipe<T> set = GetDbSet<T>(src);
            return default;
        }
        public static string ToSql<T>(this IDbSet<T> src) 
        {
            ISqlBuilder builder = new SqlBuilder();
            return builder.Build(src, new SqlOptions { Db = DbType.Mysql });
        }
    }
}
