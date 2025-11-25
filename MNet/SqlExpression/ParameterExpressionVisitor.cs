using System;
using System.Linq.Expressions;

namespace MNet.SqlExpression
{
    /// <summary>
    /// 访问表达式中的参数表达式
    /// </summary>
    public class ParameterExpressionVisitor : ExpressionVisitor
    {
        public ParameterExpressionVisitor()
        { }
        public ParameterExpressionVisitor(Func<ParameterExpression, ParameterExpression> replace)
        {
            this._replace = replace;
        }


        private Func<ParameterExpression, ParameterExpression> _replace;

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (this._replace != null)
                return this._replace(node);
            return node;
        }


        public static Expression VisitParameter(Expression expr, Func<ParameterExpression, ParameterExpression> replace)
        {
            return new ParameterExpressionVisitor(replace).Visit(expr);
        }
    }
}
