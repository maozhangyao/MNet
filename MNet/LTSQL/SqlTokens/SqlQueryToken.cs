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
        internal SqlQueryToken(TableDescriptor table, LTSQLToken from, LTSQLToken where, LTSQLToken group, LTSQLToken having, LTSQLToken order, LTSQLToken page, LTSQLToken select, bool priority)
        {
            this.IsPriority = priority;
            this.Table = table;
            this.From = from;
            this.Where = where;
            this.Group = group;
            this.Having = having;
            this.Order = order;
            this.Page = page;
            this.Select = select;
        }


        //from
        public LTSQLToken From { get; }
        //where
        public LTSQLToken Where { get; }
        //group
        public LTSQLToken Group { get; }
        //having
        public LTSQLToken Having { get; }
        //order
        public LTSQLToken Order { get; }
        //select
        public LTSQLToken Select { get; }
        //分页子句
        public LTSQLToken Page { get; }

        public Type MappingType => base.ValueType;
        public TableDescriptor Table { get; }
        public LTSQLToken this[string key] => this.Table?.Fields?.FirstOrDefault(p => p.Field == key)?.Value;


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
        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitSqlQueryToken(this);
        }
        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            
            LTSQLToken from = this.From?.Visit(visitor);
            LTSQLToken where = this.Where?.Visit(visitor);
            LTSQLToken group = this.Group?.Visit(visitor);
            LTSQLToken having = this.Having?.Visit(visitor);
            LTSQLToken order = this.Order?.Visit(visitor);
            LTSQLToken page = this.Page?.Visit(visitor);
            LTSQLToken select = this.Select?.Visit(visitor);

            return new SqlQueryToken(this.Table, from, where, group, having, order, page, select, this.IsPriority);
        }
        protected override string ToString(string fmt)
        {
            StringBuilder b = new StringBuilder();
            if (this.Select != null)
                b.AppendLine(this.Select.ToString());
            if (this.From != null)
                b.AppendLine(this.From.ToString());
            if (this.Where != null)
                b.AppendLine(this.Where.ToString());
            if (this.Group != null)
                b.AppendLine(this.Group.ToString());
            if (this.Having != null)
                b.AppendLine(this.Having.ToString());
            if (this.Order != null)
                b.AppendLine(this.Order.ToString());
            if (this.Page != null)
                b.AppendLine(this.Page.ToString());

            return string.Format(fmt, b.ToString());
        }
    }
}
