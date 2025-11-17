using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Text;

namespace MNet.SqlExpression
{
    /// <summary>
    /// 数据集, 需要表达出sql的数据集
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDbSet<T> : IEnumerable<T>
    {
    }
}
