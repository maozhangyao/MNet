namespace MNet.LTSQL.SqlTokens
{
    /// <summary>
    /// 排序项目
    /// </summary>
    public class OrderByItemToken : LTSQLToken
    {
        internal OrderByItemToken(LTSQLToken field, bool desc)
        {
            this.Field = field;
            this.Desc = desc;
        }

        public readonly LTSQLToken Field;
        public readonly bool Desc;

        protected internal override LTSQLToken Visit(LTSQLTokenVisitor visitor)
        {
            return visitor.VisitOrderByItemToken(this);
        }

        protected internal override LTSQLToken VisitChildren(LTSQLTokenVisitor visitor)
        {
            var newField = this.Field?.Visit(visitor);
            if (newField != null && !object.ReferenceEquals(newField, this.Field))
            {
                return new OrderByItemToken(newField, this.Desc);
            }
            return this;
        }

        public override string ToString()
        {
            return $"{this.Field} {(this.Desc ? "DESC" : "ASC")}";
        }
    }
}
