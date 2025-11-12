using System;
using System.Linq.Expressions;

namespace MNet.SqlExpression
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlSelectBuilder : ExpressionVisitor
    {
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return base.VisitLambda(node);
        }



        public string Build(Expression expr)
        {
            LambdaExpression lambda = expr as LambdaExpression;
            if (lambda == null)
                throw new Exception($"selet 空表达式");
            if (!(lambda.Body is ParameterExpression p) && !(lambda.Body is NewExpression))
                throw new Exception($"select 无效表达式：{expr}");

            return "*";
        }
    }
}
