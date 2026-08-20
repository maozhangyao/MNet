using MNet.LTSQL.SqlQueryStructs;
using MNet.Utils;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MNet.LTSQL
{
    public static class LTSQLNonQueryableExtensions
    {
        /// <summary>
        /// update 操作
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="setUpdate">update 的 set 语句</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ILTSQLNonQueryable<T> AsUpdate<T>(Expression<Func<T, object>> setUpdate)
        {
            if (setUpdate == null)
                throw new ArgumentNullException(nameof(setUpdate));

            return AsUpdate(null, null, setUpdate);
        }
        /// <summary>
        /// update 操作，支持设置表名以及表架构
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="schema"></param>
        /// <param name="setUpdate"></param>
        /// <returns></returns>
        public static ILTSQLNonQueryable<T> AsUpdate<T>(string table, string schema, Expression<Func<T, object>> setUpdate)
        {
            return AsUpdate<T>(default, setUpdate, table, schema);
        }
        /// <summary>
        /// update操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">在后续钩子函数中引用的实例</param>
        /// <param name="setUpdate"></param>
        /// <param name="table"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ILTSQLNonQueryable<T> AsUpdate<T>(this T entity, Expression<Func<T, object>> setUpdate, string table = null, string schema = null)
        {
            if (setUpdate == null)
                throw new ArgumentNullException(nameof(setUpdate));

            return new LTSQLObject<T>(new UpdatePart()
            {
                Refer = entity,
                Schema = schema,
                TableName = table,
                MappingType = typeof(T)
            }).SetUpdate(setUpdate);
        }
        public static ILTSQLNonQueryable<T> SetUpdate<T>(this ILTSQLNonQueryable<T> nonQuery, Expression<Func<T, object>> setUpdate)
        {
            if (setUpdate == null)
                throw new ArgumentNullException(nameof(setUpdate));

            UpdatePart part = nonQuery.Query as UpdatePart;
            if (part == null)
                throw new Exception($"非法的{nameof(QueryPart)}");

            part = part.CopyNew() as UpdatePart;
            part.SetUpdate = setUpdate;
            return new LTSQLObject<T>(part);
        }

        /// <summary>
        /// delete 操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ILTSQLNonQueryable<T> AsDelete<T>()
        {
            return AsDelete<T>(null);
        }
        /// <summary>
        /// delete 操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public static ILTSQLNonQueryable<T> AsDelete<T>(Expression<Func<T, bool>> where)
        {
            return AsDelete<T>(null, null, where);
        }
        public static ILTSQLNonQueryable<T> AsDelete<T>(string table, string schema, Expression<Func<T, bool>> where)
        {
            return AsDelete(default, where, table, schema);
        }
        /// <summary>
        /// delete 操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">后续钩子函数中引用</param>
        /// <param name="where"></param>
        /// <param name="table"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public static ILTSQLNonQueryable<T> AsDelete<T>(this T entity, Expression<Func<T, bool>> where = null, string table = null, string schema = null)
        {
            LTSQLObject<T> obj = new LTSQLObject<T>(new DeletePart()
            {
                Refer = entity,
                Schema = schema,
                TableName = table,
                MappingType = typeof(T),
            });

            return Where((ILTSQLNonQueryable<T>)obj, where);
        }

        /// <summary>
        /// 为 update, delete 操作设置where子句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nonQuery"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static ILTSQLNonQueryable<T> Where<T>(this ILTSQLNonQueryable<T> nonQuery, Expression<Func<T, bool>> expr)
        {
            NonQueryPart part = nonQuery.Query as NonQueryPart;
            if (part == null)
                throw new Exception($"非法的{nameof(QueryPart)}");
            if (expr == null)
                throw new ArgumentNullException(nameof(expr));

            part = part.CopyNew() as NonQueryPart;
            if (part.Where == null)
            {
                part.Where = expr;
                return new LTSQLObject<T>(part);
            }

            part.Where = ExpressionUtils.MergeAnd((Expression<Func<T, bool>>)part.Where, expr);
            return new LTSQLObject<T>(part);
        }

    }
}
