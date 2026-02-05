using System;
using System.Linq.Expressions;

namespace MNet.LTSQL.v1
{
    public class ExpressionModifier : ExpressionVisitor
    {

        private Func<Expression, Expression> _parameterModifier;

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Expression expr = null;
            if (_parameterModifier != null)
            {
                expr = _parameterModifier(node);
                base.VisitParameter(node);
            }
            else
            {
                expr = base.VisitParameter(node);
            }
            return expr;
        }

        public Expression VisitParameter(Expression expr, Func<Expression, Expression> modifier)
        {
            this._parameterModifier = modifier;
            
            Expression updated = this.Visit(expr);
            
            this._parameterModifier = null;

            return updated;
        }
    }
}
