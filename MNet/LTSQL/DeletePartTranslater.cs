using MNet.LTSQL.Objects;
using MNet.LTSQL.SqlQueryStructs;
using MNet.LTSQL.SqlTokens;
using MNet.Utils;
using System;

#if NET6_0_OR_GREATER
using System.ComponentModel.DataAnnotations.Schema;
#endif

namespace MNet.LTSQL
{
    /// <summary>
    /// Delete 语句翻译
    /// </summary>
    public class DeletePartTranslater : ExpressionTranslater, IQueryTranslater
    {
        public LTSQLToken Translate(QueryPart query, LTSQLTranslateScope scope)
        {
            DeletePart del = query as DeletePart;
            if(del == null)
                throw new Exception($"{nameof(DeletePartTranslater)}仅支持翻译{nameof(DeletePart)}");

            this.ApplyScope(scope);

            this.Context.Part = query;
            this.Context.Options.GetTableName ??= GetTableName;
            this.Context.Options.GetColumnName ??= GetColumnName;
            return this.TranslateDeleteCore(del);
        }

        // delete 翻译
        private LTSQLToken TranslateDeleteCore(DeletePart part)
        {
            //翻译表信息
            TableDescriptor tableDescriptor = this.TranslateTableByType(part.MappingType);
            TableObjectToken tableObjToken = LTSQLTokenFactory.CreateTableObjectToken(tableDescriptor.TableName, tableDescriptor, tableDescriptor.MappingType);

            //if (part.Where != null)
            //    this.Context.SetScopeParameter(part.Where.AsLambda().TakeParamter(0).Name, tableObjToken);

            LTSQLToken deleteClause = LTSQLTokenFactory.CreateClauseToken("DELETE FROM", tableObjToken);

            //where
            LTSQLToken whereClause = null;
            if (part.Where != null)
            {
                LTSQLToken where = this.TranslateLambda(part.Where.AsLambda(), tableObjToken);
                whereClause = LTSQLTokenFactory.CreateWhereClauseToken(where);
            }

            LTSQLToken deleteClauseToken = whereClause != null ? SequenceToken.Create(deleteClause, whereClause) : SequenceToken.Create(deleteClause);
            return PostTranslate(deleteClauseToken);
        }
    }
}
