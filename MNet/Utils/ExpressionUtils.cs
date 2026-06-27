using MNet.LTSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MNet.Utils
{
    public static class ExpressionUtils
    {
        public static LambdaExpression AsLambda(this Expression expr)
        {
            return expr as LambdaExpression;
        }
        public static ParameterExpression TakeParamter(this LambdaExpression expr, int paramterIndex)
        {
            if (expr.Parameters.Count <= paramterIndex)
                throw new ArgumentOutOfRangeException(nameof(paramterIndex), $"参数索引 {paramterIndex} 超出范围，表达式参数数量为 {expr.Parameters.Count}");
            return expr.Parameters[paramterIndex];
        }
        public static Expression<Func<T, bool>> MergeAnd<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            if (expr1 == null && expr2 == null)
                throw new ArgumentNullException(nameof(expr1) + " and " + nameof(expr2));
            if (expr1 == null || expr2 == null)
                return expr1 ?? expr2;

            ExpressionModifier modifier = new ExpressionModifier();
            Expression newExpr1Body = modifier.ModifyParameter(expr1.Body, expr1.TakeParamter(0), expr2.TakeParamter(0));

            return Expression.Lambda<Func<T, bool>>(
                    Expression.And(newExpr1Body, expr2.Body), expr2.TakeParamter(0)
                );
        }
        public static Expression<Func<T, bool>> MergeOr<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            if (expr1 == null && expr2 == null)
                throw new ArgumentNullException(nameof(expr1) + " and " + nameof(expr2));
            if (expr1 == null || expr2 == null)
                return expr1 ?? expr2;

            ExpressionModifier modifier = new ExpressionModifier();
            Expression newExpr1Body = modifier.ModifyParameter(expr1.Body, expr1.TakeParamter(0), expr2.TakeParamter(0));

            return Expression.Lambda<Func<T, bool>>(
                    Expression.Or(newExpr1Body, expr2.Body), expr2.TakeParamter(0)
                );
        }
    }
}
