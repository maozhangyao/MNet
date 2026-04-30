using System;
using MNet.LTSQL.SqlTokenExtends;

namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 表示集合操作
    /// </summary>
    public class DataSetToken : SqlValueToken, ISelectable
    {
        public DataSetToken(Type valueType, IEnumerable<LTSQLToken> querys, DbSetType type, bool distinct)
        {
            this.Querys = querys.ToArray();
        }

        //数组形式表示，而不是使用树结构表示节点，是为了避免递归时栈溢出
        //因为有可能有大量的 单 select 硬编码语句如：
        // select 1 union select 2 union select 3 .... union select 1000000
        // 这种情况，如果使用树结构，那么就会很可能导致栈溢出
        public LTSQLToken[] Querys { get; }
        public DbSetType SetType { get; }
        public bool Distinct { get; }

        Type ISelectable.MappingType => this.ValueType;
        public IEnumerable<FieldInfoToken> Fields { get; set; }


        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitDataSetToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            var arr = new LTSQLToken[this.Querys.Length];
            for (int i = 0; i < this.Querys.Length; i++)
            {
                arr[i] = visitor.Visit(this.Querys[i]);
            }

            return new DataSetToken(this.ValueType, arr, this.SetType, this.Distinct);
        }
    }
}

