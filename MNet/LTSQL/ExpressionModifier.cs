using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using MNet.Utils;

namespace MNet.LTSQL
{
    public class ExpressionModifier : ExpressionVisitor
    {
        public ExpressionModifier()
        {
            this._lambdaScopeStk = new List<LambdaScope>();
            this._modifiers1 = new Dictionary<ExpressionType, Func<Expression, Expression>>();
            this._modifiers2 = new Dictionary<ExpressionType, Func<Expression, Expression>>();
        }

        private class LambdaScope
        {
            public ParameterExpression[] Parameters;
            public bool HasParameter(string parameterName)
            {
                return this.Parameters != null && this.Parameters.Any(p => p.Name == parameterName);
            }
            public bool HasParameter(string parameterName, Type parameterType)
            {
                return this.Parameters != null && this.Parameters.Any(p => p.Name == parameterName && p.Type == parameterType);
            }
        }

        //lambda作用域层级
        private List<LambdaScope> _lambdaScopeStk;
        //装饰_modifiers2
        private Dictionary<ExpressionType, Func<Expression, Expression>> _modifiers1;
        //实际的表达式替换逻辑
        private Dictionary<ExpressionType, Func<Expression, Expression>> _modifiers2;
        


        private void PushScopeStk(LambdaExpression lambda)
        {
            LambdaScope scope = new LambdaScope();
            scope.Parameters = lambda.Parameters?.ToArray();
            this._lambdaScopeStk.Add(scope);
        }
        private void PushScopeStk(Expression lambdaOrBody)
        {
            if (lambdaOrBody is LambdaExpression lambda)
            {
                this.PushScopeStk(lambda);
            }
            else
            {
                LambdaScope scope = new LambdaScope();
                scope.Parameters = null;
                this._lambdaScopeStk.Add(scope);
            }
        }
        private void PopScopeStk()
        {
            this._lambdaScopeStk.RemoveAt(this._lambdaScopeStk.Count - 1);
        }
        private bool HasParameterScope(string parameterName)
        {
            int cnt = this._lambdaScopeStk.Count;
            //排除第一个，因为第一个是自身作用域
            for (int i = cnt - 1; i > 0; i--)
            {
                LambdaScope scope = this._lambdaScopeStk[i];
                if (scope.HasParameter(parameterName))
                    return true;
            }
            return false;
        }
        private bool HasParameterScope(string parameterName, Type parameterType)
        {
            int cnt = this._lambdaScopeStk.Count;
            //排除第一个，因为第一个是自身作用域
            for (int i = cnt - 1; i > 0; i--)
            {
                LambdaScope scope = this._lambdaScopeStk[i];
                if (scope.HasParameter(parameterName, parameterType))
                    return true;
            }
            return false;
        }



        protected override Expression VisitParameter(ParameterExpression node)
        {
            Func<Expression, Expression> modifier = null;
            if (this._modifiers1.TryGetValue(node.NodeType, out modifier))
                return modifier(node);

            return base.VisitParameter(node);
        }
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            this.PushScopeStk(node);
            Expression expr = base.VisitLambda(node);
            this.PopScopeStk();
            return expr;
        }


        public ExpressionModifier WithModifer(ExpressionType exprType, Func<Expression, Expression> modifier)
        {
            this._modifiers2[exprType] = modifier;
            return this;
        }
        public ExpressionModifier WithParameterModifier(Func<Expression, Expression> modifier)
        {
            return WithModifer(ExpressionType.Parameter, modifier);
        }
        /// <summary>
        /// 修改lambd的参数
        /// </summary>
        /// <param name="lambda"></param>
        /// <param name="parameterIdx"></param>
        /// <param name="excludeFormalParamter">是否不需要替换形参</param>
        /// <returns></returns>
        public LambdaExpression ModifyParameter(LambdaExpression lambda, int parameterIdx, bool excludeFormalParamter = false)
        {
            if (lambda.Parameters == null || lambda.Parameters.Count <= 0)
                return lambda;

            ParameterExpression parameter = lambda.Parameters[parameterIdx];
            if (excludeFormalParamter)
            {
                Expression expr = this.ModifyParameter(lambda.Body, parameter);
                return Expression.Lambda(expr, lambda.Parameters.ToArray());
            }
            return this.ModifyParameter(lambda, parameter) as LambdaExpression;
        }
        public LambdaExpression ModifyParameter(LambdaExpression lambda, string parameterName, bool excludeFormalParamter = false)
        {
            if (lambda.Parameters == null || lambda.Parameters.Count <= 0)
                return lambda;

            ParameterExpression parameter = lambda.Parameters.FirstOrDefault(p => p.Name == parameterName);
            if (parameter == null)
                throw new Exception($"lambda中没有参数名为{parameterName}的参数.");

            if (excludeFormalParamter)
            {
                Expression expr = this.ModifyParameter(lambda.Body, parameter);
                return Expression.Lambda(expr, lambda.Parameters.ToArray());
            }
            return this.ModifyParameter(lambda, parameter) as LambdaExpression;
        }
        public Expression ModifyParameter(Expression expr, ParameterExpression oldParameter)
        {
            this.PushScopeStk(expr);

            this._modifiers1[ExpressionType.Parameter] = expr =>
            {
                if (expr is ParameterExpression p
                    && oldParameter.Name == p.Name
                    && this._modifiers2.TryGetValue(p.NodeType, out var modifier)
                    && !this.HasParameterScope(p.Name) //具有相同参数名参数的作用域下，无需替换
                   )
                {
                    return modifier(expr);
                }
                return expr;
            };

            this.PopScopeStk();
            return this.Visit(expr);
        }
    }
}
