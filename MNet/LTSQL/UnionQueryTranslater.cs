using System;
using MNet.LTSQL.Objects;
using MNet.LTSQL.SqlQueryStructs;
using MNet.LTSQL.SqlTokenExtends;
using MNet.LTSQL.SqlTokens;

namespace MNet.LTSQL
{
    /// <summary>
    /// 联合查询翻译器
    /// </summary>
    public class UnionQueryTranslater : IQueryTranslater
    {
        public LTSQLToken Translate(QueryPart query, LTSQLOptions options)
        {
            return this.Translate(query, new LTSQLTranslateScope(LTSQLContext.Create(options)));
        }

        public LTSQLToken Translate(QueryPart query, LTSQLTranslateScope scope)
        {
            QuerySetPart set = query as QuerySetPart;
            if (set == null)
                throw new Exception($"不支持的查询类型：{query.GetType().Name}; 当前翻译器{nameof(UnionQueryTranslater)}只支持查询类型{nameof(QuerySetPart)}");

            List<LTSQLToken> rts = new List<LTSQLToken>();
            IQueryTranslaterFactory factory = new QueryTranslaterFactory();
            foreach (QueryPart sub in set.Querys)
            {
                IQueryTranslater translater = factory.Create(sub);
                LTSQLToken ret = translater.Translate(sub, scope.NewScope())?.TryPriority(false);
                if (sub is QuerySetPart)
                {
                    //如果子查询是集合操作，则需要添加优先级计算(sqllite好像不支持集合操作时加优先级运算，有点坑)
                    ret = ret is IPriorable prior && !prior.IsPriority ? prior.SetPriority(true) as LTSQLToken : ret;
                }
                rts.Add(ret);
            }

            ISelectable select = rts[0] as ISelectable;
            if (select == null)
                throw new Exception($"不支持的查询类型：{rts[0].GetType().Name}，非{nameof(ISelectable)}接口");

            TableDescriptor descriptor = new TableDescriptor("$temp", "", select.Table.MappingType);
            foreach (var field in select.Table.Fields)
            {
                descriptor.AddField(new FieldDescriptor(field.Field, field.Value, field.FieldValueType));
            }

            return new DataSetToken(query.MappingType, rts, set.SetType, set.Distinct)
            {
                Table = descriptor
            };
        }
    }
}

