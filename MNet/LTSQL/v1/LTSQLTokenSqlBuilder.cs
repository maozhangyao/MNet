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
            .UseTokenBuilder<ObjectToken>((t, ctx, nxt) =>
            {
                if (t.ObjectType == SqlObjectType.Table)
                    ctx.Sql.Append(ctx.SqlKeyWordEscap(t.Alias, ctx));
                else
                    ctx.Sql.Append(t.Alias);
            })
            .UseTokenBuilder<AliasToken>((t, ctx, nxt) =>
            {
                nxt(t.Object);
                ctx.Sql.Append(" AS ");
                ctx.Sql.Append(ctx.SqlKeyWordEscap(t.Alias, ctx));
            })
            .UseTokenBuilder<BoolCalcToken>((t, ctx, nxt) =>
            {
                nxt(t.Left); //可能为 null， 如 Exists， Not Exists 操作
                ctx.Sql.Append(' ');
                ctx.Sql.Append(t.Opration);
                ctx.Sql.Append(' ');
                nxt(t.Right);

            })
            .UseTokenBuilder<BinaryToken>((t, ctx, nxt) =>
            {
                nxt(t.Left);
                ctx.Sql.Append(' ');
                ctx.Sql.Append(t.Opration);
                ctx.Sql.Append(' ');
                nxt(t.Right);

            })
            .UseTokenBuilder<ConstantToken>((t, ctx, nxt) =>
            {
                ctx.Sql.Append(t.Value);
            })
            .UseTokenBuilder<SyntaxToken>((t, ctx, nxt) =>
            {
                ctx.Sql.Append(t.EscapeKey ? ctx.SqlKeyWordEscap(t.Text, ctx) : t.Text);

            })
            .UseTokenBuilder<NullToken>((t, ctx, nxt) =>
            {
                ctx.Sql.Append(t.Value);
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
            .UseTokenBuilder<FunctionCallToken>((t, ctx, nxt) =>
            {
                nxt(t.FunctionObject);
                ctx.Sql.Append("(");
                if (t.Parameters != null)
                {
                    bool comma = false;
                    foreach (LTSQLToken arg in t.Parameters)
                    {
                        if (comma)
                            ctx.Sql.Append(", ");
                        else
                            comma = true;
                        nxt(arg);
                    }
                }
                ctx.Sql.Append(")");

            })
            .UseTokenBuilder<ObjectAccessToken>((t, ctx, nxt) =>
            {
                nxt(t.Object);
                ctx.Sql.Append('.');
                ctx.Sql.Append(ctx.SqlKeyWordEscap(t.Prop, ctx));
            })
            .UseTokenBuilder<SelectToken>((t, ctx, nxt) =>
            {
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
            .UseTokenBuilder<SqlParameterToken>((t, ctx, nxt) =>
            {
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
            .UseTokenBuilder<SqlQueryToken>((t, ctx, nxt) =>
            {
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
            .UseTokenBuilder<SqlScopeToken>((t, ctx, nxt) =>
            {
                ctx.Sql.Append('(');
                nxt(t.Inner);
                ctx.Sql.Append(')');

            })
            .UseTokenBuilder<PriorityCalcToken>((t, ctx, nxt) =>
            {
                ctx.Sql.Append('(');
                nxt(t.Value);
                ctx.Sql.Append(')');
            })
            .UseTokenBuilder<SequenceToken>((t, ctx, nxt) =>
            {
                foreach (LTSQLToken token in t)
                    nxt(token);
            })
            .UseTokenBuilder<PageToken>((t, ctx, nxt) =>
            {
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
            (Type, Action<LTSQLToken, SqlBuilderContext, Action<LTSQLToken>>) item = (type, (t, b, nxt) =>
            {
                if (t is T t1)
                    builder(t1, b, nxt);
            }
            );

            if (index >= 0)
                this._builders[index] = item;
            else
                this._builders.Add(item);

            return this;
        }

    }
}
