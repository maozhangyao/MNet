using System;
using System.Linq.Expressions;

namespace MNet.SqlExpression
{
    /// <summary>
    /// 表达式操作
    /// </summary>
    public static class ExpressionExtions
    {
        /// <summary>
        ///  将表达式 and 拼接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr"></param>
        /// <param name="newParameterName">指定一个新的参数名称</param>
        /// <param name="ands"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr, string newParameterName, params Expression<Func<T, bool>>[] ands)
        {
            if (expr == null)
                throw new ArgumentNullException(nameof(expr));
            
            //统一的参数名
            ParameterExpression _new = Expression.Parameter(typeof(T), newParameterName);

            
            expr = expr.RenameParameter(_new); //统一参数
            if (ands != null)
            {
                foreach(Expression<Func<T, bool>> and in ands)
                {
                    if (and == null)
                        continue;
                    //统一参数
                    Expression<Func<T, bool>> _and = and.RenameParameter(_new);
                    //拼接
                    expr = Expression.Lambda<Func<T, bool>>(
                        Expression.AndAlso(expr.Body, _and.Body),
                        _new
                    );
                }
            }

            return expr;
        }

        /// <summary>
        /// 参数重命名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr"></param>
        /// <param name="renamed"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Expression<Func<T, Result>> RenameParameter<T, Result>(this Expression<Func<T, Result>> expr, string newParameterName)
        {
            if (expr == null)
                throw new ArgumentNullException(nameof(expr));

            ParameterExpression _new = Expression.Parameter(typeof(T), newParameterName);
            return expr.RenameParameter(_new);
        }

        /// <summary>
        /// 参数重命名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expr"></param>
        /// <param name="newParameter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Expression<Func<T, Result>> RenameParameter<T, Result>(this Expression<Func<T, Result>> expr, ParameterExpression newParameter)
        {
            if (expr == null)
                throw new ArgumentNullException(nameof(expr));

            ParameterExpression _old = expr.Parameters[0];
            ParameterExpression _new = newParameter;
            return ParameterExpressionVisitor.VisitParameter(expr, p =>
            {
                if (_old == p)
                    return _new;
                return p;
            }) as Expression<Func<T, Result>>;
        }

    }
}
