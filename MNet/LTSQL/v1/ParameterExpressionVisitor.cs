using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MNet.LTSQL.v1
{
    public class ParameterExpressionVisitor
    {
    }

    public class ExpressionModifier : ExpressionVisitor
    {

        private Func<Expression, Expression> _parameterModifier;

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Expression expr = null;
            if (_parameterModifier != null)
                expr = _parameterModifier(node);

            Expression temp = base.VisitParameter(node);
            return expr ?? temp;
        }

        public void VisitParameter(Expression expr, Func<Expression, Expression> modifier)
        {
            this._parameterModifier = modifier;
            this.Visit(expr);
            this._parameterModifier = null;
        }
    }
}
