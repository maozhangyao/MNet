using System;
using MNet.LTSQL.Objects;
using MNet.LTSQL.SqlQueryStructs;
using MNet.LTSQL.SqlTokenExtends;
using MNet.LTSQL.SqlTokens;
using MNet.LTSQL.TypeModels;
using MNet.Utils;

namespace MNet.LTSQL
{
    /// <summary>
    /// update 语句翻译
    /// </summary>
    public class UpdatePartTranslater : ExpressionTranslater, IQueryTranslater
    {
        // update 翻译
        private LTSQLToken TranslateUpdateCore(UpdatePart part)
        {
            //翻译表信息
            EntityTypeDescriptor tableDescriptor = this.GetEntityTypeDescriptor(part.MappingType, part.Schema, part.TableName, part.Refer);
            TableObjectToken tableObjToken = LTSQLTokenFactory.CreateTableObjectToken(tableDescriptor.TableName, tableDescriptor, tableDescriptor.Type);

            ITupleable tuple = this.TranslateLambda(part.SetUpdate.AsLambda(), tableObjToken) as ITupleable;
            if (tuple == null)
                throw new Exception($"无法翻译Update表达式：{part.SetUpdate}");

            //where
            LTSQLToken whereClause = null;
            if (part.Where != null)
                whereClause = this.TranslateLambda(part.Where.AsLambda(), tableObjToken);

            UpdateClauseToken updateClause = LTSQLTokenFactory.CreateUpdateClauseToken(tableObjToken, tuple, whereClause);
            return PostTranslate(updateClause);
        }

        public LTSQLToken Translate(QueryPart query, LTSQLTranslateScope scope)
        {
            UpdatePart upd = query as UpdatePart;
            if (upd == null)
                throw new Exception($"{nameof(UpdatePartTranslater)}仅支持翻译{nameof(UpdatePart)}");

            this.ApplyScope(scope);

            this.Context.Part = query;
            return this.TranslateUpdateCore(upd);
        }

    }
}
