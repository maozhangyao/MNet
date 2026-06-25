using MNet.LTSQL.SqlTokenExtends;
using MNet.LTSQL.SqlTokens;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
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
            this._commons = new List<(Type, Action<LTSQLToken, SqlWriterContext, Action>)>(8);
            this._specials = new List<(Type, Action<LTSQLToken, SqlWriterContext, Action<LTSQLToken>>)>(32);
        }


        private List<(Type, Action<LTSQLToken, SqlWriterContext, Action>)> _commons;
        private List<(Type, Action<LTSQLToken, SqlWriterContext, Action<LTSQLToken>>)> _specials;

        //默认的
        public static LTSQLTokenSqlBuilder Default => UseDefault();



        private void NextAtCommon(LTSQLToken token, SqlWriterContext context, int pos)
        {
            Type tokenType = token.GetType();
            //顺序执行
            int index = pos >= this._commons.Count ? -1 : this._commons.FindIndex(pos + 1, p => p.Item1.IsAssignableFrom(tokenType) /*tokenType.IsAssignableTo(p.Item1)*/);
            if (index < 0)
            {
                this.NextAtSpecial(token, context);
            }
            else
            {
                this._commons[index].Item2(token, context, () => this.NextAtCommon(token, context, index));
            }
        }
        private void NextAtSpecial(LTSQLToken token, SqlWriterContext context)
        {
            Type type = token.GetType();
            foreach (var item in this._specials)
            {
                if (item.Item1 == type)
                {
                    item.Item2(token, context, t => this.Next(t, context));
                    return;
                }
            }
        }
        //执行 builder
        private void Next(LTSQLToken token, SqlWriterContext context)
        {
            if (token == null)
                return;

            if (context.TokenStack.Count > 0)
                context.ParentToken = context.TokenStack.Peek();

            context.TokenStack.Push(token);
            this.NextAtCommon(token, context, -1);
            context.TokenStack.Pop();
        }


        //初始化默认的 sql 生成器
        private static LTSQLTokenSqlBuilder UseDefault()
        {
            LTSQLTokenSqlBuilder builder = new LTSQLTokenSqlBuilder();

            builder
            .UseCommonToken<IPriorable>((t, ctx, nxt) => {
                if (t.IsPriority)
                    ctx.Writer.Write('(');

                nxt();

                if (t.IsPriority)
                    ctx.Writer.Write(')');
            })
            .UseSpecialToken<ObjectToken>((t, ctx, nxt) =>
            {
                if (t.ObjectType == SqlObjectType.Table)
                    ctx.Writer.Write(ctx.SqlKeyWordEscape(t.Alias, ctx));
                else
                    ctx.Writer.Write(t.Alias);
            })
            .UseSpecialToken<TableObjectToken>((t, ctx, nxt) => {
                ctx.Writer.Write(ctx.SqlKeyWordEscape(t.Alias, ctx));
            })
            .UseSpecialToken<AliasToken>((t, ctx, nxt) =>
            {
                nxt(t.Object);
                ctx.Writer.Write(" AS ");
                ctx.Writer.Write(ctx.SqlKeyWordEscape(t.Alias, ctx));
            })
            .UseSpecialToken<BoolCalcToken>((t, ctx, nxt) =>
            {
                nxt(t.Left); //可能为 null， 如 Exists， Not Exists 操作
                ctx.Writer.WriteWhite();
                ctx.Writer.Write(t.Opration);
                ctx.Writer.WriteWhite();
                nxt(t.Right);
            })
            .UseSpecialToken<BinaryToken>((t, ctx, nxt) =>
            {
                nxt(t.Left);
                ctx.Writer.WriteWhite();
                ctx.Writer.Write(t.Opration);
                ctx.Writer.WriteWhite();
                nxt(t.Right);

            })
            .UseSpecialToken<ConstantToken>((t, ctx, nxt) =>
            {
                ctx.Writer.Write(t.Value);
            })
            .UseSpecialToken<SyntaxToken>((t, ctx, nxt) =>
            {
                ctx.Writer.Write(t.EscapeKey ? ctx.SqlKeyWordEscape(t.Text, ctx) : t.Text);

            })
            .UseSpecialToken<NullToken>((t, ctx, nxt) =>
            {
                ctx.Writer.Write(t.Value);
            })
            .UseSpecialToken<JoinToken>((t, ctx, nxt) =>
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

                ctx.Writer.WriteWhite();

                nxt(t.JoinQuery);

                ctx.Writer.Write(" ON ");

                nxt(t.JoinKeys);
            })
            .UseSpecialToken<FunctionCallToken>((t, ctx, nxt) =>
            {
                nxt(t.FunctionName);
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
            .UseSpecialToken<ObjectAccessToken>((t, ctx, nxt) =>
            {
                nxt(t.Object);
                ctx.Writer.Write('.');
                ctx.Writer.Write(ctx.SqlKeyWordEscape(t.Prop, ctx));
            })
            .UseSpecialToken<SqlParameterToken>((t, ctx, nxt) =>
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
            .UseSpecialToken<SqlQueryToken>((t, ctx, nxt) =>
            {
                nxt(t.Select);
                
                if (t.From != null)
                {
                    ctx.Writer.WriteLine();
                    nxt(t.From);
                }
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
            .UseSpecialToken<PriorityCalcToken>((t, ctx, nxt) =>
            {
                ctx.Writer.Write('(');
                if (t.Value is ISelectable)
                    ctx.Writer.BeginScope();

                nxt(t.Value);

                if (t.Value is ISelectable)
                    ctx.Writer.EndScope();
                ctx.Writer.Write(')');
            })
            .UseSpecialToken<SequenceToken>((t, ctx, nxt) =>
            {
                foreach (LTSQLToken token in t)
                    nxt(token);
            })
            .UseSpecialToken<ListToken>((t, ctx, nxt) =>
            {
                if (t.Tokens == null)
                    return;

                ClauseToken parent = ctx.ParentToken as ClauseToken;
                bool newLineFlag = parent != null && parent.ClauseName.ToLower() switch
                {
                    "from" => true,
                    "select" => true,
                    "order by" => true,
                    "group by" => true,
                    _ => false
                };

                if (newLineFlag)
                    ctx.Writer.BeginScope("  ");

                for (int i = 0; i < t.Tokens.Length; i++)
                {
                    nxt(t.Tokens[i]);
                    if (i + 1 < t.Tokens.Length)
                    {
                        ctx.Writer.Write(", ");
                        if (newLineFlag)
                            ctx.Writer.WriteLine();
                    }
                    else
                    {
                        //ctx.Writer.WriteWhite();
                    }
                }

                if (newLineFlag)
                    ctx.Writer.EndScope();
            })
            .UseSpecialToken<PageToken>((t, ctx, nxt) =>
            {
                if (ctx.DbType == DbTypes.MySQL || ctx.DbType == DbTypes.SQLLite)
                {
                    // 使用 limit 子句分页
                    if (t.Take != null)
                    {
                        ctx.Writer.Write("LIMIT ");
                        ctx.Writer.Write(t.Take);
                        ctx.Writer.WriteWhite();
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

            })
            .UseSpecialToken<ClauseToken>((t, ctx, nxt) =>
            {
                ctx.Writer.WriteWhite(t.ClauseName);
                if (t.SubClause != null)
                {
                    foreach (var sub in t.SubClause)
                    {
                        nxt(sub);
                        ctx.Writer.WriteWhite();
                    }
                }
            })
            .UseSpecialToken<TupleToken>((t, ctx, nxt) =>
            {
                ctx.Writer.Write("(");
                bool flag = false;
                foreach (LTSQLToken item in t.Props)
                {
                    if (flag)
                        ctx.Writer.Write(", ");
                    flag = true;
                    nxt(item);
                }
                ctx.Writer.Write(")");
            })
            .UseSpecialToken<DataSetToken>((t, ctx, nxt) =>
            {
                for (int i = 0; i < t.Querys.Length; i++)
                {
                    if (i > 0)
                    {
                        ctx.Writer.WriteLine();
                        if (t.SetType == DbSetType.Union)
                            ctx.Writer.Write("UNION ");
                        else if (t.SetType == DbSetType.Intersect)
                            ctx.Writer.Write("INTERSECT ");
                        else if (t.SetType == DbSetType.Except)
                            ctx.Writer.Write(ctx.DbType == DbTypes.Oracle ? "MINUS " : "EXCEPT ");
                        else
                            throw new Exception($"不支持的SetOperatorType:{t.SetType}");
                        if (!t.Distinct)
                            ctx.Writer.Write("ALL ");
                        ctx.Writer.WriteLine();
                    }
                    nxt(t.Querys[i]);
                }
            })
            .UseSpecialToken<SwitchCaseToken>((t, ctx, nxt) => {

                ctx.Writer.Write("CASE ");
                ctx.Writer.BeginScope();
                
                ctx.Writer.Write("WHEN ");
                nxt(t.When);
                ctx.Writer.Write(" THEN ");
                nxt(t.ThenValue);

                ctx.Writer.WriteLine();
                ctx.Writer.Write("ELSE ");
                nxt(t.ThenElse);
                //ctx.Writer.WriteLine();

                ctx.Writer.EndScope();
                ctx.Writer.Write("END ");
            });

            return builder;
        }


        /// <summary>
        /// 构造sql
        /// </summary>
        /// <param name="token"></param>
        /// <param name="context"></param>
        public void Build(LTSQLToken token, SqlBuilderOptions context)
        {
            SqlWriterContext writerCxt = new SqlWriterContext();
            writerCxt.DbType = context.DbType;
            writerCxt.UseParameter = context.UseParameter;
            writerCxt.SqlParameters = context.SqlParameters ?? new List<(string key, object value)>(8);
            //writerCxt.TokenStack = new Stack<LTSQLToken>();
            writerCxt.Writer = (context.SqlWriterFactory ?? (() => new LTSQLWriter(false, null)))();
            writerCxt.Obj2SqlPart = context.Obj2SqlPart ?? ((obj, ctx) => DbUtils.ToSqlPart(obj, ctx.DbType));
            writerCxt.SqlKeyWordEscape = context.SqlKeyWordEscape ?? ((t, ctx) => DbUtils.Escape(t, ctx.DbType));

            this.Next(token, writerCxt);

            //生成sql
            context.Sql = writerCxt.Writer.GetSqlBuilder();
        }
        public LTSQLTokenSqlBuilder UseCommonToken<T>(Action<T, SqlWriterContext, Action> builder)
        {
            if (builder == null)
                return this;

            Type type = typeof(T);
            int index = this._commons.FindIndex(p => p.Item1 == type);
            (Type, Action<LTSQLToken, SqlWriterContext, Action>) item = (type, (t, b, nxt) =>
            {
                if (t is T t1)
                    builder(t1, b, nxt);
            }
            );

            if (index >= 0)
                this._commons[index] = item;
            else
                this._commons.Add(item);

            return this;
        }
        /// <summary>
        /// 使用对应的token的builder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public LTSQLTokenSqlBuilder UseSpecialToken<T>(Action<T, SqlWriterContext, Action<LTSQLToken>> builder) where T : LTSQLToken
        {
            if (builder == null)
                return this;

            Type type = typeof(T);
            int index = this._specials.FindIndex(p => p.Item1 == type);
            (Type, Action<LTSQLToken, SqlWriterContext, Action<LTSQLToken>>) item = (type, (t, b, nxt) =>
            {
                if (t is T t1)
                    builder(t1, b, nxt);
            }
            );

            if (index >= 0)
                this._specials[index] = item;
            else
                this._specials.Add(item);

            return this;
        }

    }
}
