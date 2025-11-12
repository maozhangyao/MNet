using System.Linq.Expressions;

namespace MNet.SqlExpression
{
    /// <summary>
    /// 表达数据集排序
    /// </summary>
    public class DbSetOrder
    {
        public DbSetOrder()
        { }
        public DbSetOrder(Expression keyExpr, bool desc = false)
        {
            this.OrderByExpress = keyExpr;
            this.IsDesc = desc;
        }


        //排序表达式
        //Expressions<Func<T,Tkey>>
        public Expression OrderByExpress { get; set; }
        //是否降序
        public bool IsDesc { get; set; }
    }
}
