using MNet.LTSQL.v1.SqlTokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.v1
{
    /// <summary>
    /// 默认的 LTSQLToken to sql 提供
    /// </summary>
    public class LTSQLTokenSqlBuilder : ISqlBuilder
    {
        public LTSQLTokenSqlBuilder()
        {
            this._builders = new List<(Type, Action<LTSQLToken, SqlBuilderContext, Action<LTSQLToken>>)>(32);
        }


        private List<(Type, Action<LTSQLToken, SqlBuilderContext, Action<LTSQLToken>>)> _builders;

        //默认的
        public static LTSQLTokenSqlBuilder Default => UseDefault();


        //执行 builder
        private void Next(LTSQLToken token, SqlBuilderContext context)
        {
            if (token == null)
                return;

            Type type = token.GetType();
            foreach (var item in this._builders)
            {
                if (item.Item1 == type)
                {
                    item.Item2(token, context, t => this.Next(t, context));
                    return;
                }
            }
        }
        //初始化默认的 sql 生成器
        private static LTSQLTokenSqlBuilder UseDefault()
        {
            LTSQLTokenSqlBuilder builder = new LTSQLTokenSqlBuilder();

            builder
            .UseTokenBuilder<AliasTableToken>((t, ctx, nxt) =>
            {
                nxt(t.Query);
                ctx.Sql.Append(' ');
                ctx.Sql.Append(ctx.SqlKeyWordEscap(t.Alias, ctx));

            })
            .UseTokenBuilder<AliasToken>((t, ctx, nxt) =>
            {
                ctx.Sql.Append(ctx.SqlKeyWordEscap(t.Alias, ctx));

            })
            .UseTokenBuilder<BogusToken>((t, ctx, nxt) =>
            {
                //需要报错，因为 bogusToken 不应该存在
                throw new Exception($"存在{nameof(BogusToken)}，无法翻译成对应SQL");
            })
            .UseTokenBuilder<ConditionToken>((t, ctx, nxt) => {
                nxt(t.Left); //可能为 null， 如 Exists， Not Exists 操作
                ctx.Sql.Append(' ');
                ctx.Sql.Append(t.ConditionType);
                ctx.Sql.Append(' ');
                nxt(t.Right);

            })
            .UseTokenBuilder<ConstantToken>((t, ctx, nxt) => {
                ctx.Sql.Append(t.Value);
            })
            .UseTokenBuilder<NullToken>((t, ctx, nxt) =>
            {
                ctx.Sql.Append(t.Value);
            })
            .UseTokenBuilder<FromToken>((t, ctx, nxt) => {
                ctx.Sql.Append("FROM ");
                nxt(t.Source);

            })
            .UseTokenBuilder<JoinToken>((t, ctx, nxt) =>
            {
                nxt(t.MainQuery);

                ctx.Sql.AppendLine();
                if (t.JoinType == JoinType.InnerJoin)
                    ctx.Sql.Append("INNER JOIN");
                else if (t.JoinType == JoinType.LeftJoin)
                    ctx.Sql.Append("LEFT JOIN");
                else if (t.JoinType == JoinType.RightJoin)
                    ctx.Sql.Append("RIGHT JOIN");
                else
                    ctx.Sql.Append(t.JoinType);

                ctx.Sql.Append(' ');

                nxt(t.JoinQuery);

                ctx.Sql.Append(" ON ");

                nxt(t.JoinKeys);
            })
            .UseTokenBuilder<FunctionToken>((t, ctx, nxt) => {
                ctx.Sql.Append(t.FunctionName);
                ctx.Sql.Append('(');

                if(t.Parameters != null)
                {
                    bool comma = false;
                    foreach (var param in t.Parameters)
                    {
                        if (comma)
                            ctx.Sql.Append(',');
                        nxt(param);
                        comma = true;
                    }
                }
                
                ctx.Sql.Append(')');

            })
            //.UseTokenBuilder<GroupObjToken>((t, ctx, nxt) => { 
                
            //})
            .UseTokenBuilder<GroupToken>((t, ctx, nxt) => {
                ctx.Sql.Append("GROUP BY ");
                bool comma = false;
                foreach (LTSQLToken item in t.GroupByItems)
                {
                    if (comma)
                        ctx.Sql.Append(", ");
                    comma = true;
                    nxt(item);
                }

            })
            .UseTokenBuilder<LTSQLToken>((t, ctx, nxt) => { 
                //理论上不会被调用
            })
            .UseTokenBuilder<ObjectAccessToken>((t, ctx, nxt) => {
                nxt(t.Owner);
                ctx.Sql.Append('.');
                ctx.Sql.Append(ctx.SqlKeyWordEscap(t.Field, ctx));

            })
            .UseTokenBuilder<OrderByItemToken>((t, ctx, nxt) => {
                nxt(t.Item);
                ctx.Sql.Append(' ');
                if (!t.IsAsc)
                    ctx.Sql.Append("desc");

            })
            .UseTokenBuilder<OrderToken>((t, ctx, nxt) => {
                ctx.Sql.Append("ORDER BY ");
                nxt(t.OrderBy);

            })
            .UseTokenBuilder<SelectToken>((t, ctx, nxt) => {
                ctx.Sql.Append("SELECT ");

                if (t.Distinct)
                {
                    ctx.Sql.Append("DISTINCT ");
                }

                if (t.MSSQLTopStatement != null)
                {
                    ctx.Sql.Append("TOP ");
                    ctx.Sql.Append(t.MSSQLTopStatement);
                    ctx.Sql.Append(' ');
                }

                if (t.AllFields)
                {
                    ctx.Sql.Append("*");
                }
                else
                {
                    nxt(t.Fields);
                }

            })
            .UseTokenBuilder<SelectItemToken>((t, ctx, nxt) => {
                nxt(t.Field);
                ctx.Sql.Append(' ');
                if(!string.IsNullOrEmpty(t.FieldAlias))
                    ctx.Sql.Append(ctx.SqlKeyWordEscap(t.FieldAlias, ctx));
            })
            .UseTokenBuilder<SqlParameterToken>((t, ctx, nxt) => {
                //是否参数化
                if (ctx.UseParameter)
                {
                    if (!t.ParameterName.StartsWith('@'))
                        ctx.Sql.Append('@');

                    ctx.Sql.Append(t.ParameterName);
                    ctx.SqlParameters.Add(t.ParameterName, t.Value);
                }
                else
                {
                    ctx.Sql.Append(ctx.Obj2SqlPart(t.Value, ctx));
                }

            })
            .UseTokenBuilder<SqlQueryToken>((t, ctx, nxt) => {
                nxt(t.Select);
                ctx.Sql.AppendLine();
                nxt(t.From);
                
                if (t.Where != null)
                {
                    ctx.Sql.AppendLine();
                    nxt(t.Where);
                }
                if (t.Group != null)
                {
                    ctx.Sql.AppendLine();
                    nxt(t.Group);
                }
                if (t.Having != null)
                {
                    ctx.Sql.AppendLine();
                    nxt(t.Having);
                }
                if (t.Order != null)
                {
                    ctx.Sql.AppendLine();
                    nxt(t.Order);
                }
                if (t.Page != null)
                {
                    ctx.Sql.AppendLine();
                    nxt(t.Page);
                }

            })
            .UseTokenBuilder<SqlScopeToken>((t, ctx, nxt) => {
                ctx.Sql.Append('(');
                nxt(t.Inner);
                ctx.Sql.Append(')');

            })
            //.UseTokenBuilder<SqlValueToken>((t, ctx, nxt) => { })
            .UseTokenBuilder<TokenItemListToken>((t, ctx, nxt) => {
                if (t.Items == null)
                    return;

                for (int i = 0; i < t.Items.Length; i++)
                {
                    if (i > 0)
                        ctx.Sql.Append(t.Separator);

                    LTSQLToken item = t.Items[i];
                    nxt(item);
                }

            })
            .UseTokenBuilder<WhereToken>((t, ctx, nxt) => {
                ctx.Sql.Append(t.WhereOrHaving);
                ctx.Sql.Append(" ");
                nxt(t.Condition);

            })
            .UseTokenBuilder<PageToken>((t, ctx, nxt) => {
                if (ctx.DbType == DbType.MySQL || ctx.DbType == DbType.SQLLite)
                {
                    // 使用 limit 子句分页
                    if (t.Take != null)
                    {
                        ctx.Sql.Append("LIMIT ");
                        ctx.Sql.Append(t.Take);
                        ctx.Sql.Append(' ');
                    }
                    if (t.Skip != null)
                    {
                        ctx.Sql.Append("OFFSET ");
                        ctx.Sql.Append(t.Skip);
                    }
                }
                else
                {
                    // 使用 fetch next 子句分页
                    if (t.Skip != null)
                    {
                        ctx.Sql.Append("OFFSET ");
                        ctx.Sql.Append(t.Skip);
                        ctx.Sql.Append(" ROWS ");
                    }
                    if (t.Take != null)
                    {
                        ctx.Sql.Append("FETCH NEXT ");
                        ctx.Sql.Append(t.Take);
                        ctx.Sql.Append(" ROWS ONLY");
                    }
                }

            });


            return builder;
        }


        /// <summary>
        /// 构造sql
        /// </summary>
        /// <param name="token"></param>
        /// <param name="context"></param>
        public void Build(LTSQLToken token, SqlBuilderContext context)
        {
            context.Sql ??= new StringBuilder(1024);
            context.SqlParameters ??= new Dictionary<string, object>();

            context.Obj2SqlPart ??= (obj, ctx) => DbUtils.ToSqlPart(obj, ctx.DbType);
            context.SqlKeyWordEscap ??= (t, ctx) => DbUtils.Escape(t, ctx.DbType);

            this.Next(token, context);
        }
        /// <summary>
        /// 使用对应的token的builder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public LTSQLTokenSqlBuilder UseTokenBuilder<T>(Action<T, SqlBuilderContext, Action<LTSQLToken>> builder) where T : LTSQLToken
        {
            if (builder == null)
                return this;

            Type type = typeof(T);
            int index = this._builders.FindIndex(p => p.Item1 == type);
            (Type, Action<LTSQLToken, SqlBuilderContext, Action<LTSQLToken>>) item = (type, (t, b, nxt) => {
                if (t is T t1)
                    builder(t1, b, nxt);
            });

            if (index >= 0)
                this._builders[index] = item;
            else
                this._builders.Add(item);

            return this;
        }
     
    }
}
