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
    }
}
