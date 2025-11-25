using System;
using System.Linq.Expressions;

namespace MNet.SqlExpression
{
    /// <summary>
    /// sql 作用域，用于表示当前sql所处的作用域，在子查询过程中，能够通过作用域链条找到父级sql作用域
    /// </summary>
    public class SqlScope
    {
        public SqlScope()
        { }
        public SqlScope(SqlDescriptor descriptor)
        {
            this.Descriptor = descriptor;
        }
        public SqlScope(SqlScope parent, SqlDescriptor descriptor) : this(descriptor)
        {
            this.ParentScope = parent;
        }


        /// <summary>
        /// 父级sql作用域
        /// </summary>
        public SqlScope ParentScope { get; set; }
        /// <summary>
        /// 当前的sql作用域中sql结构
        /// </summary>
        public SqlDescriptor Descriptor { get; set; }

        /// <summary>
        /// 判断参数是否处于当前作用域中
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsScope(ParameterExpression p)
        {
            DbSetStrcut set = this.Descriptor.Define;
            return p.Name == set.FromNamed.Name && p.Type == set.FromNamed.Type;
        }
    }
}
