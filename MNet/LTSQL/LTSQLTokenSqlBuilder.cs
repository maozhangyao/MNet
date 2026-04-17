using MNet.LTSQL.SqlTokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL
{
    /// <summary>
    /// 默认的 LTSQLToken to sql 提供
    /// </summary>
    public class LTSQLTokenSqlBuilder : ISqlBuilder
    {
        public LTSQLTokenSqlBuilder()
        {
            this._builders = new List<(Type, Action<LTSQLToken, SqlWriterContext, Action<LTSQLToken>>)>(32);
        }


        private List<(Type, Action<LTSQLToken, SqlWriterContext, Action<LTSQLToken>>)> _builders;

        //默认的
        public static LTSQLTokenSqlBuilder Default => UseDefault();


        //执行 builder
        private void Next(LTSQLToken token, SqlWriterContext context)
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
                    ctx.Writer.Write(ctx.SqlKeyWordEscape(t.Alias, ctx));
                else
                    ctx.Writer.Write(t.Alias);
            })
            .UseTokenBuilder<AliasToken>((t, ctx, nxt) =>
            {
                nxt(t.Object);
                ctx.Writer.Write(" AS ");
                ctx.Writer.Write(ctx.SqlKeyWordEscape(t.Alias, ctx));
            })
            .UseTokenBuilder<BoolCalcToken>((t, ctx, nxt) =>
            {
                nxt(t.Left); //可能为 null， 如 Exists， Not Exists 操作
                ctx.Writer.Write(' ');
                ctx.Writer.Write(t.Opration);
                ctx.Writer.Write(' ');
                nxt(t.Right);

            })
            .UseTokenBuilder<BinaryToken>((t, ctx, nxt) =>
            {
                nxt(t.Left);
                ctx.Writer.Write(' ');
                ctx.Writer.Write(t.Opration);
                ctx.Writer.Write(' ');
                nxt(t.Right);

            })
            .UseTokenBuilder<ConstantToken>((t, ctx, nxt) =>
            {
                ctx.Writer.Write(t.Value);
            })
            .UseTokenBuilder<SyntaxToken>((t, ctx, nxt) =>
            {
                ctx.Writer.Write(t.EscapeKey ? ctx.SqlKeyWordEscape(t.Text, ctx) : t.Text);

            })
            .UseTokenBuilder<NullToken>((t, ctx, nxt) =>
            {
                ctx.Writer.Write(t.Value);
            })
            .UseTokenBuilder<JoinToken>((t, ctx, nxt) =>
            {
                nxt(t.MainQuery);

                ctx.Writer.WriteLine();
                if (t.JoinType == JoinType.InnerJoin)
                    ctx.Writer.Write("INNER JOIN");
                else if (t.JoinType == JoinType.LeftJoin)
                    ctx.Writer.Write("LEFT JOIN");
                else if (t.JoinType == JoinType.RightJoin)
                    ctx.Writer.Write("RIGHT JOIN");
                else
                    ctx.Writer.Write(t.JoinType);

                ctx.Writer.Write(' ');

                nxt(t.JoinQuery);

                ctx.Writer.Write(" ON ");

                nxt(t.JoinKeys);
            })
            .UseTokenBuilder<FunctionCallToken>((t, ctx, nxt) =>
            {
                nxt(t.FunctionObject);
                ctx.Writer.Write("(");
                if (t.Parameters != null)
                {
                    bool comma = false;
                    foreach (LTSQLToken arg in t.Parameters)
                    {
                        if (comma)
                            ctx.Writer.Write(", ");
                        else
                            comma = true;
                        nxt(arg);
                    }
                }
                ctx.Writer.Write(")");

            })
            .UseTokenBuilder<ObjectAccessToken>((t, ctx, nxt) =>
            {
                nxt(t.Object);
                ctx.Writer.Write('.');
                ctx.Writer.Write(ctx.SqlKeyWordEscape(t.Prop, ctx));
            })
            .UseTokenBuilder<SelectToken>((t, ctx, nxt) =>
            {
                ctx.Writer.Write("SELECT ");

                if (t.Distinct)
                {
                    ctx.Writer.Write("DISTINCT ");
                }

                if (t.TopLimit != null)
                {
                    ctx.Writer.Write("TOP ");
                    ctx.Writer.Write(t.TopLimit);
                    ctx.Writer.Write(' ');
                }

                if (t.Asterisk)
                {
                    ctx.Writer.Write("*");
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
                        ctx.Writer.Write('@');

                    ctx.Writer.Write(t.ParameterName);
                    ctx.AddParameter(t.ParameterName, t.Value);
                }
                else
                {
                    ctx.Writer.Write(ctx.Obj2SqlPart(t.Value, ctx));
                }

            })
            .UseTokenBuilder<SqlQueryToken>((t, ctx, nxt) =>
            {
                nxt(t.Select);
                ctx.Writer.WriteLine();
                nxt(t.From);

                if (t.Where != null)
                {
                    ctx.Writer.WriteLine();
                    nxt(t.Where);
                }
                if (t.Group != null)
                {
                    ctx.Writer.WriteLine();
                    nxt(t.Group);
                }
                if (t.Having != null)
                {
                    ctx.Writer.WriteLine();
                    nxt(t.Having);
                }
                if (t.Order != null)
                {
                    ctx.Writer.WriteLine();
                    nxt(t.Order);
                }
                if (t.Page != null)
                {
                    ctx.Writer.WriteLine();
                    nxt(t.Page);
                }

            })
            .UseTokenBuilder<SqlScopeToken>((t, ctx, nxt) =>
            {
                ctx.Writer.Write('(');
                ctx.Writer.BeginIndent();
                nxt(t.Inner);
                ctx.Writer.EndIndent();
                ctx.Writer.Write(')');
            })
            .UseTokenBuilder<PriorityCalcToken>((t, ctx, nxt) =>
            {
                ctx.Writer.Write('(');
                if (t.Value is SqlQueryToken)
                    ctx.Writer.BeginIndent();

                nxt(t.Value);
                
                if (t.Value is SqlQueryToken)
                    ctx.Writer.EndIndent();
                ctx.Writer.Write(')');
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
                        ctx.Writer.Write("LIMIT ");
                        ctx.Writer.Write(t.Take);
                        ctx.Writer.Write(' ');
                    }
                    if (t.Skip != null)
                    {
                        ctx.Writer.Write("OFFSET ");
                        ctx.Writer.Write(t.Skip);
                    }
                }
                else
                {
                    // 使用 fetch next 子句分页
                    if (t.Skip != null)
                    {
                        ctx.Writer.Write("OFFSET ");
                        ctx.Writer.Write(t.Skip);
                        ctx.Writer.Write(" ROWS ");
                    }
                    if (t.Take != null)
                    {
                        ctx.Writer.Write("FETCH NEXT ");
                        ctx.Writer.Write(t.Take);
                        ctx.Writer.Write(" ROWS ONLY");
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
            SqlWriterContext writerCxt = new SqlWriterContext();
            writerCxt.DbType = context.DbType;
            writerCxt.UseParameter = context.UseParameter;
            writerCxt.SqlParameters = context.SqlParameters ?? new List<(string key, object value)>(8);

            writerCxt.Writer = (context.SqlWriterFactory ?? (() => new LTSQLWriter(false, null)))();
            writerCxt.Obj2SqlPart = context.Obj2SqlPart ?? ((obj, ctx) => DbUtils.ToSqlPart(obj, ctx.DbType));
            writerCxt.SqlKeyWordEscape = context.SqlKeyWordEscape ?? ((t, ctx) => DbUtils.Escape(t, ctx.DbType));

            this.Next(token, writerCxt);

            //生成sql
            context.Sql = writerCxt.Writer.GetSqlBuilder();
        }
        /// <summary>
        /// 使用对应的token的builder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public LTSQLTokenSqlBuilder UseTokenBuilder<T>(Action<T, SqlWriterContext, Action<LTSQLToken>> builder) where T : LTSQLToken
        {
            if (builder == null)
                return this;

            Type type = typeof(T);
            int index = this._builders.FindIndex(p => p.Item1 == type);
            (Type, Action<LTSQLToken, SqlWriterContext, Action<LTSQLToken>>) item = (type, (t, b, nxt) =>
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
