using System;
using System.Linq;
using System.Collections.Generic;

namespace MNet.LTSQL.SqlQueryStructs
{
    /// <summary>
    /// 集合操作：union, intersect, except
    /// </summary>
    public class QuerySetPart : QueryPart
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="querys">同类型查询</param>
        /// <param name="type">集合操作类型</param>
        /// <param name="distinct">
        /// 集合操作是否需要去重，如：union表示去重，union all表示不去重。
        /// 除了union操所有数据都支持不去重外，其他的操作符并非所有数据都支持不去重，所以使用前需要明了对应数据库是否支持不去重
        /// </param>
        public QuerySetPart(Type mapppingType, IEnumerable<QueryPart> querys, DbSetType type, bool distinct = true)
        {
            //使用构造函数参数赋值对应的属性值
            this.SetType = type;
            this.Distinct = distinct;
            this.Querys = querys.ToArray();
            this.MappingType = mapppingType;
        }

        public bool Distinct { get; }
        public DbSetType SetType { get; }
        public QueryPart[] Querys { get; }

        public override QueryPart CopyNew()
        {
            return new QuerySetPart(this.MappingType, Querys.Select(q => q.CopyNew()), SetType, Distinct)
            {
                Alias = this.Alias
            };
        }
    }
}