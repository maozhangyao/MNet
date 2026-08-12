using MNet.LTSQL.Objects;
using MNet.LTSQL.SqlQueryStructs;
using MNet.LTSQL.SqlTokenExtends;
using MNet.LTSQL.SqlTokens;
using MNet.Utils;
using System;
#if NET6_0_OR_GREATER
using System.ComponentModel.DataAnnotations.Schema;
#endif

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
            TableDescriptor tableDescriptor = this.TranslateTableByType(part.MappingType);
            TableObjectToken tableObjToken = LTSQLTokenFactory.CreateTableObjectToken(tableDescriptor.TableName, tableDescriptor, tableDescriptor.MappingType);

            if (part.Where != null)
                this.Context.SetScopeParameter(part.Where.AsLambda().TakeParamter(0).Name, tableObjToken);

            ITupleable tuple = this.TranslateLambda(part.UpdateSet.AsLambda(), tableObjToken) as ITupleable;
            if (tuple == null)
                throw new Exception($"无法翻译Update表达式：{part.UpdateSet}");

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
            this.Context.Options.GetTableName ??= GetTableName;
            this.Context.Options.GetColumnName ??= GetColumnName;

            return this.TranslateUpdateCore(upd);
        }

    }
}
