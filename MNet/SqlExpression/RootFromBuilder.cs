using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MNet.SqlExpression
{
    /// <summary>
    /// 根节点的from
    /// </summary>
    public class RootFromBuilder
    {
        public string Build(Expression expr)
        {
            LambdaExpression lambda = expr as LambdaExpression;
            if (lambda == null || !(lambda.Body is ParameterExpression p))
                throw new Exception($"无效表达式：{expr}");

            Type t = p.Type;
            return DbUtils.Escape(t.Name, DbType.Mysql);
        }
    }
}
