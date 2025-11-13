using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace MNet.SqlExpression
{
    public static class DbSetExtensions
    {
        private static DbPipe<T> GetDbSet<T>(IDbSet<T> src) where T : class
        {
            var set = src as DbPipe<T>;
            if (set == null)
                throw new Exception($"无效参数{nameof(src)}");
            return set;
        }
        private static IDbSet<T> AddOrder<T, TKey>(IDbSet<T> src, Expression<Func<T, TKey>> keyExpre, bool isDesc) where T : class
        {
            DbPipe<T> set = GetDbSet(src);
            if (keyExpre == null)
                throw new Exception($"空条件表达式：{nameof(keyExpre)}");

            set.AddOrder(keyExpre, isDesc);
            return set;
        }


        public static IDbSet<T> AsDbSet<T>() where T : class
        {
            DbSet<T, T> head = new DbSet<T, T>();
            return head.Select(p => p);//初始的from，即自身
        }
        public static IDbSet<T> AsDbSet<T>(this T entity) where T : class
        {
            return AsDbSet<T>();
        }
        public static IDbSet<T> Where<T>(this IDbSet<T> src, Expression<Func<T, bool>> expr) where T : class
        {
            DbPipe<T> set = GetDbSet(src);
            if (expr == null)
                throw new Exception($"空条件表达式：{nameof(expr)}");

            set.AddWhere(expr);
            return src;
        }
        public static IDbSet<T> OrderBy<T, TKey>(this IDbSet<T> src, Expression<Func<T, TKey>> keyExpre) where T : class
        {
            return AddOrder(src, keyExpre, false);
        }
        public static IDbSet<T> ThenBy<T, TKey>(this IDbSet<T> src, Expression<Func<T, TKey>> keyExpre) where T : class
        {
            return AddOrder(src, keyExpre, false);
        }
        public static IDbSet<T> OrderByDescending<T, TKey>(this IDbSet<T> src, Expression<Func<T, TKey>> keyExpre) where T : class
        {
            return AddOrder(src, keyExpre, true);
        }
        public static IDbSet<T> ThenByDescending<T, TKey>(this IDbSet<T> src, Expression<Func<T, TKey>> keyExpre) where T : class
        {
            return AddOrder(src, keyExpre, true);
        }
        public static IDbSet<TResult> Select<T, TResult>(this IDbSet<T> src, Expression<Func<T, TResult>> expr) where T : class where TResult : class
        {
            if (src == null)
                throw new Exception($"Select {nameof(src)}参数不能为空。");

            DbPipe<T> db = GetDbSet(src);
            db.AddSelect(expr);

            DbSet<T,TResult> set = new DbSet<T, TResult>(db);
            set.AddSelect(p => p);
            return set;
        }
        public static string ToSql<T>(this IDbSet<T> src) where T : class
        {
            ISqlBuilder builder = new SqlBuilder();
            return builder.Build(src, new SqlOptions { Db = DbType.Mysql });
        }
    }
}
