using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using MNet.LTSQL.SqlTokenExtends;
using MNet.LTSQL.Objects;
using System.Collections;

namespace MNet.LTSQL.SqlTokens
{
    public class SqlQueryToken : SqlValueToken, ISelectable
    {
        internal SqlQueryToken() 
        { }
        internal SqlQueryToken(bool priority)
        {
            this.IsPriority = priority;
        }


        public LTSQLToken this[string key] => this.Table?.Fields?.FirstOrDefault(p => p.Field == key)?.Value;
        public LTSQLToken From { get; set; }
        public LTSQLToken Where { get; set; }
        public LTSQLToken Group { get; set; }
        public LTSQLToken Having { get; set; }
        public LTSQLToken Order { get; set; }
        public LTSQLToken Select { get; set; }
        //分页子句
        public LTSQLToken Page { get; set; }
        public Type MappingType => base.ValueType;
        
        public TableDescriptor Table { get; set; }


        public IEnumerator<(string key, LTSQLToken value)> GetEnumerator()
        {
            return this.Table.Fields.Select(f => (f.Field, f.Value)).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public Type GetValueType(string key)
        {
            return this.Table.Fields?.FirstOrDefault(p => p.Field == key)?.FieldValueType;
        }

        public override IPriorable SetPriority(bool isPriority)
        {
            this.IsPriority = isPriority;
            return this;
        }
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitSqlQueryToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            SqlQueryToken sub = new SqlQueryToken();
            sub.IsPriority = this.IsPriority;
            sub.Table = this.Table;
            sub.ValueType = this.ValueType;

            sub.From = this.From?.Visit(visitor);
            sub.Where = this.Where?.Visit(visitor);
            sub.Group = this.Group?.Visit(visitor);
            sub.Having = this.Having?.Visit(visitor);
            sub.Order = this.Order?.Visit(visitor);
            sub.Page = this.Page?.Visit(visitor);
            sub.Select = this.Select?.Visit(visitor);

            return sub;
        }
    }
}
