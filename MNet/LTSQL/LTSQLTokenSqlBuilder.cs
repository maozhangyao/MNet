using System;
using System.Collections.Generic;
using MNet.LTSQL.SqlTokenExtends;
using MNet.LTSQL.SqlTokens;

namespace MNet.LTSQL
{
    /// <summary>
    /// 默认的 LTSQLToken to sql 提供
    /// </summary>
    public class LTSQLTokenSqlBuilder : ISqlBuilder
    {
        public LTSQLTokenSqlBuilder()
        {
            this._newCommons = new List<(Func<LTSQLToken, bool>, Action<LTSQLToken, SqlWriterContext, Action>)>(8);
            this._newSpecials = new List<(Func<LTSQLToken, bool>, Action<LTSQLToken, SqlWriterContext, Action<LTSQLToken>>)>(32);
        }



        //通用的翻译逻辑
        private List<(Func<LTSQLToken, bool>, Action<LTSQLToken, SqlWriterContext, Action>)> _newCommons;
        //特定的翻译逻辑
        private List<(Func<LTSQLToken, bool>, Action<LTSQLToken, SqlWriterContext, Action<LTSQLToken>>)> _newSpecials;



        //默认的
        public static LTSQLTokenSqlBuilder Default => UseDefault();
        //初始化默认的 sql 生成器
        private static LTSQLTokenSqlBuilder UseDefault()
        {
            LTSQLTokenSqlBuilder builder = new LTSQLTokenSqlBuilder();

            builder
            .UseCommonByType<IPriorable>((t, ctx, nxt) =>
            {
                if (t.IsPriority)
                    ctx.Writer.Write('(');

                nxt();

                if (t.IsPriority)
                    ctx.Writer.Write(')');
            })
            .UseSpecialByType<ObjectToken>((t, ctx, nxt) =>
            {
                if (t.ObjectType == SqlObjectType.Table)
                    ctx.Writer.Write(ctx.SqlKeyWordEscape(t.Alias, ctx));
                else
                    ctx.Writer.Write(t.Alias);
            })
            .UseSpecialByType<TableObjectToken>((t, ctx, nxt) =>
            {
                ctx.Writer.Write(ctx.SqlKeyWordEscape(t.Alias, ctx));
            })
            .UseSpecialByType<AliasToken>((t, ctx, nxt) =>
            {
                nxt(t.Object);
                ctx.Writer.Write(" AS ");
                ctx.Writer.Write(ctx.SqlKeyWordEscape(t.Alias, ctx));
            })
            .UseSpecialByType<BoolCalcToken>((t, ctx, nxt) =>
            {
                nxt(t.Left); //可能为 null， 如 Exists， Not Exists 操作
                ctx.Writer.WriteWhite();
                ctx.Writer.Write(t.Opration);
                ctx.Writer.WriteWhite();
                nxt(t.Right);
            })
            .UseSpecialByType<BinaryToken>((t, ctx, nxt) =>
            {
                nxt(t.Left);
                ctx.Writer.WriteWhite();
                ctx.Writer.Write(t.Opration);
                ctx.Writer.WriteWhite();
                nxt(t.Right);

            })
            .UseSpecialByType<ConstantToken>((t, ctx, nxt) =>
            {
                ctx.Writer.Write(t.Value);
            })
            .UseSpecialByType<SyntaxToken>((t, ctx, nxt) =>
            {
                ctx.Writer.Write(t.EscapeKey ? ctx.SqlKeyWordEscape(t.Text, ctx) : t.Text);

            })
            .UseSpecialByType<NullToken>((t, ctx, nxt) =>
            {
                ctx.Writer.Write(t.Value);
            })
            .UseSpecialByType<JoinToken>((t, ctx, nxt) =>
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
            .UseSpecialByType<FunctionCallToken>((t, ctx, nxt) =>
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
            .UseSpecialByType<ObjectAccessToken>((t, ctx, nxt) =>
            {
                nxt(t.Object);
                ctx.Writer.Write('.');
                ctx.Writer.Write(ctx.SqlKeyWordEscape(t.Prop, ctx));
            })
            .UseSpecialByType<SqlParameterToken>((t, ctx, nxt) =>
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
            .UseSpecialByType<SqlQueryToken>((t, ctx, nxt) =>
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
            .UseSpecialByType<PriorityCalcToken>((t, ctx, nxt) =>
            {
                ctx.Writer.Write('(');
                if (t.Value is ISelectable)
                    ctx.Writer.BeginScope();

                nxt(t.Value);

                if (t.Value is ISelectable)
                    ctx.Writer.EndScope();
                ctx.Writer.Write(')');
            })
            .UseSpecialByType<SequenceToken>((t, ctx, nxt) =>
            {
                foreach (LTSQLToken token in t)
                    nxt(token);
            })
            .UseSpecialByType<ListToken>((t, ctx, nxt) =>
            {
                if (t.Tokens == null)
                    return;

                ClauseToken parent = ctx.ParentToken as ClauseToken;
                bool newLineFlag = parent != null && parent.Clause.ToLower() switch
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
            .UseSpecialByType<PageToken>((t, ctx, nxt) =>
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
            .UseSpecial(t => t is ClauseToken, (t, ctx, nxt) =>
            {
                ClauseToken clause = (ClauseToken)t; 
                ctx.Writer.WriteWhite(clause.Clause);
                if (clause.SubClause != null)
                {
                    foreach (var sub in clause.SubClause)
                    {
                        nxt(sub);
                        ctx.Writer.WriteWhite();
                    }
                }
            })
            .UseSpecialByType<FromClauseToken>((t, ctx, nxt) =>
            {
                ctx.Writer.WriteWhite(t.Clause);
                if (t.SubClause != null)
                {
                    foreach (var sub in t.SubClause)
                    {
                        nxt(sub);
                        ctx.Writer.WriteWhite();
                    }
                }
            })
            .UseSpecialByType<WhereClauseToken>((t, ctx, nxt) =>
            {
                ctx.Writer.WriteWhite(t.Clause);
                if (t.SubClause != null)
                {
                    foreach (var sub in t.SubClause)
                    {
                        nxt(sub);
                        ctx.Writer.WriteWhite();
                    }
                }
            })
            .UseSpecialByType<TupleToken>((t, ctx, nxt) =>
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
            .UseSpecialByType<SetOperationToken>((t, ctx, nxt) =>
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
            .UseSpecialByType<SwitchCaseToken>((t, ctx, nxt) =>
            {

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
            })
            .UseSpecialByType<UpdateClauseToken>((t, ctx, nxt) =>
            {
                ctx.Writer.Write("UPDATE");
                nxt(t.Table);
                ctx.Writer.Write(" SET ");

                bool comma = false;
                foreach (var kv in t.SetClause)
                {
                    if (comma)
                        ctx.Writer.Write(",");

                    comma = true;
                    ctx.Writer.Write($"{kv.key} = ");
                    nxt(kv.value);
                    ctx.Writer.Write(" ");
                }

                if (t.WhereClause != null)
                {
                    ctx.Writer.WriteLine();
                    ctx.Writer.Write("WHERE ");
                    nxt(t.WhereClause);
                }
            });

            return builder;
        }


        //通用逻辑
        private void NextAtCommon(LTSQLToken token, SqlWriterContext context, int pos)
        {
            Type tokenType = token.GetType();
            //顺序执行
            int index = pos >= this._newCommons.Count ? -1 : this._newCommons.FindIndex(pos + 1, p => p.Item1(token));
            if (index < 0)
            {
                this.NextAtSpecial(token, context);
            }
            else
            {
                this._newCommons[index].Item2(token, context, () => this.NextAtCommon(token, context, index));
            }
        }
        //特定逻辑
        private void NextAtSpecial(LTSQLToken token, SqlWriterContext context)
        {
             //从尾部开始遍历，尾部优先级高于头部
            int cnt = this._newSpecials.Count;
            for (int i = cnt - 1; i >= 0; i--)
            {
                var item = this._newSpecials[i];
                if (item.Item1(token))
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
            context.SqlParameters = writerCxt.SqlParameters;
        }
        /// <summary>
        /// 根据类型坐等号匹配
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public LTSQLTokenSqlBuilder UseCommonByType<T>(Action<T, SqlWriterContext, Action> builder)
        {
            return this.UseCommon(t => t is T, (t, ctx, nxt) => builder((T)((object)t), ctx, nxt));
        }
        /// <summary>
        /// 使用条件匹配，如果token条件匹配则翻译，且能匹配多次
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public LTSQLTokenSqlBuilder UseCommon(Func<LTSQLToken, bool> condition, Action<LTSQLToken, SqlWriterContext, Action> builder)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));


            this._newCommons.Add((condition, builder));
            return this;
        }

        /// <summary>
        /// 使用对应的token的builder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public LTSQLTokenSqlBuilder UseSpecialByType<T>(Action<T, SqlWriterContext, Action<LTSQLToken>> builder) where T : LTSQLToken
        {
            return this.UseSpecial(t => t.GetType() == typeof(T), (t, ctx, nxt) => builder((T)t, ctx, nxt));
        }
        /// <summary>
        /// 条件编译，如果token条件匹配则翻译，且只能匹配一次
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        public LTSQLTokenSqlBuilder UseSpecial(Func<LTSQLToken, bool> condition, Action<LTSQLToken, SqlWriterContext, Action<LTSQLToken>> builder)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            this._newSpecials.Add((condition, builder));
            return this;
        }
    }
}
