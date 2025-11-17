using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace MNet.SqlExpression
{
    /// <summary>
    /// 提供逻辑表达式拼接
    /// </summary>
    public class LogicExpresionCombine : ExpressionVisitor
    {
        public LogicExpresionCombine() : this("p")
        {

        }
        public LogicExpresionCombine(string parameterName)
        {
            this._parameterName = parameterName;
        }

        //参数名
        private string _parameterName;
        private ParameterExpression _replace;
        private ReadOnlyCollection<ParameterExpression> _params;


        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (_params.Contains(node))
                return _replace;
            return node;
        }

        public Expression<Func<T,bool>> Combine<T>(params Expression<Func<T, bool>>[] exprs)
        {
            if (exprs == null || exprs.Length <= 0)
                return null;

            this._replace = Expression.Parameter(typeof(T), this._parameterName);
            
            Expression combine = null;
            foreach (Expression<Func<T, bool>> expr in exprs)
            {
                if (expr == null)
                    continue;

                this._params = expr.Parameters;
                Expression<Func<T, bool>> _new = this.Visit(expr) as Expression<Func<T, bool>>;
                combine = combine == null ? _new.Body : Expression.AndAlso(combine, _new.Body);
            }

            var result = Expression.Lambda<Func<T, bool>>(combine, this._replace);
            Console.WriteLine(result);
            return result;
        }
    }
}
