using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MNet.LTSQL.SqlTokens;
using System.Reflection;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MNet.Utils;
using System.Xml;
using System.Net.Http.Headers;

namespace MNet.LTSQL
{
    /// <summary>
    /// 对外部提供翻译扩展
    /// </summary>
    public class LTSQLTokenTranslaterSelector
    {
        public LTSQLTokenTranslaterSelector()
        {
            this._memberTranslaters = new List<Action<TranslateContext>>();
            this._expressionTranslaters = new List<Action<TranslateContext>>();
        }


        private List<Action<TranslateContext>> _memberTranslaters;
        private List<Action<TranslateContext>> _expressionTranslaters;


        public static readonly LTSQLTokenTranslaterSelector Default = GetDefaultTranslaterSelector();


        //对类型成员进行转换(优先级低)
        public virtual LTSQLTokenTranslaterSelector UseMemberTranslate(Action<TranslateContext> translate)
        {
            if (translate != null)
                this._memberTranslaters.Add(translate);
            return this;
        }
        //对表达式节点进行转换(优先级高)
        public virtual LTSQLTokenTranslaterSelector UseExpressionTranslate(Action<TranslateContext> translate)
        {
            if (translate != null)
                this._expressionTranslaters.Add(translate);
            return this;
        }


        public virtual void TranslateMember(TranslateContext context)
        {
            if (this._memberTranslaters == null)
                return;

            foreach (Action<TranslateContext> translater in this._memberTranslaters)
            {
                if (translater != null)
                    translater(context);
                if (context.ResultToken != null)
                    return;
            }
        }
        public virtual void TranslateExpression(TranslateContext context)
        {
            if (this._expressionTranslaters == null)
                return;

            foreach (Action<TranslateContext> translater in this._expressionTranslaters)
            {
                if (translater != null)
                    translater(context);
                if (context.ResultToken != null)
                    return;
            }
        }


        //内置的函数翻译支持
        private static LTSQLTokenTranslaterSelector GetDefaultTranslaterSelector()
        {
            LTSQLTokenTranslaterSelector defaultTranslater = new LTSQLTokenTranslaterSelector();

            // 聚合函数 SUM
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                MethodInfo sumMethod = ctx.Member as MethodInfo;
                if (sumMethod == null || ctx.Owner != null || ctx.Member.Name != nameof(Enumerable.Sum))
                    return;
                if (ctx.OwnerType != typeof(Enumerable) && ctx.OwnerType != typeof(LTSQLQueryableExtensions))
                    return;
                if (ctx.MethodParameterTokenList.IsEmpty())
                    return;

                // SUM 是扩展方法，所以该方法的第一个参数表示实例对象
                GroupObjToken groupObj = ctx.MethodParameterTokenList[0] as GroupObjToken;
                LTSQLToken[] parameters = ctx.MethodParameterTokenList.Skip(1).ToArray();
                if (groupObj != null)
                    ctx.ResultToken = LTSQLTokenFactory.CreateFunctionCallToken("SUM", parameters, sumMethod.ReturnType);
            });


            // 聚合函数 MAX
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                MethodInfo maxMethod = ctx.Member as MethodInfo;
                if (maxMethod == null || ctx.Owner != null || ctx.Member.Name != nameof(Enumerable.Max))
                    return;
                if (ctx.OwnerType != typeof(Enumerable) && ctx.OwnerType != typeof(LTSQLQueryableExtensions))
                    return;
                if (ctx.MethodParameterTokenList.IsEmpty())
                    return;

                // MAX 是扩展方法，所以该方法的第一个参数表示实例对象
                GroupObjToken groupObj = ctx.MethodParameterTokenList[0] as GroupObjToken;
                LTSQLToken[] parameters = ctx.MethodParameterTokenList.Skip(1).ToArray();
                if (groupObj != null)
                    ctx.ResultToken = LTSQLTokenFactory.CreateFunctionCallToken("MAX", parameters, maxMethod.ReturnType);
            });


            // 聚合函数 MIN
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                MethodInfo minMethod = ctx.Member as MethodInfo;
                if (minMethod == null || ctx.Owner != null || ctx.Member.Name != nameof(Enumerable.Min))
                    return;
                if (ctx.OwnerType != typeof(Enumerable) && ctx.OwnerType != typeof(LTSQLQueryableExtensions))
                    return;
                if (ctx.MethodParameterTokenList.IsEmpty())
                    return;

                // MIN 是扩展方法，所以该方法的第一个参数表示实例对象
                GroupObjToken groupObj = ctx.MethodParameterTokenList[0] as GroupObjToken;
                LTSQLToken[] parameters = ctx.MethodParameterTokenList.Skip(1).ToArray();
                if (groupObj != null)
                    ctx.ResultToken = LTSQLTokenFactory.CreateFunctionCallToken("MIN", parameters, minMethod.ReturnType);
            });


            // 聚合函数 AVG
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                MethodInfo avgMethod = ctx.Member as MethodInfo;
                if (avgMethod == null || ctx.Owner != null || ctx.Member.Name != nameof(Enumerable.Average))
                    return;
                if (ctx.OwnerType != typeof(Enumerable) && ctx.OwnerType != typeof(LTSQLQueryableExtensions))
                    return;
                if (ctx.MethodParameterTokenList.IsEmpty())
                    return;

                // SUM 是扩展方法，所以该方法的第一个参数表示实例对象
                GroupObjToken groupObj = ctx.MethodParameterTokenList[0] as GroupObjToken;
                LTSQLToken[] parameters = ctx.MethodParameterTokenList.Skip(1).ToArray();
                if (groupObj != null)
                    ctx.ResultToken = LTSQLTokenFactory.CreateFunctionCallToken("AVG", parameters, avgMethod.ReturnType);
            });


            // 聚合函数 COUNT
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                MethodInfo cntMethod = ctx.Member as MethodInfo;
                if (cntMethod == null || ctx.Owner != null || (ctx.Member.Name != nameof(Enumerable.Count) && ctx.Member.Name != nameof(Enumerable.LongCount)))
                    return;
                if (ctx.OwnerType != typeof(Enumerable) && ctx.OwnerType != typeof(LTSQLQueryableExtensions))
                    return;
                if (ctx.MethodParameterTokenList.IsEmpty())
                    return;

                // COUNT 是扩展方法，所以该方法的第一个参数表示实例对象
                GroupObjToken groupObj = ctx.MethodParameterTokenList[0] as GroupObjToken;
                LTSQLToken[] parameters = ctx.MethodParameterTokenList.Skip(1).ToArray();
                if (groupObj != null)
                    ctx.ResultToken = LTSQLTokenFactory.CreateFunctionCallToken("COUNT", (parameters.IsEmpty() ? new[] { SyntaxToken.Create("*") } : parameters), cntMethod.ReturnType);
            });


            // IN 操作
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                MethodInfo contains = ctx.Member as MethodInfo;
                if (contains == null)
                    return;

                if (
                    (
                      typeof(Enumerable) == ctx.OwnerType
                      || typeof(LTSQLQueryableExtensions) == ctx.OwnerType
                      || typeof(IEnumerable).IsAssignableFrom(ctx.OwnerType)
                    )
                    && typeof(string) != ctx.OwnerType
                    && (ctx.Member.Name == nameof(Enumerable.Contains)))
                {
                    // A Container B
                    // B IN A
                    LTSQLToken left = null;
                    LTSQLToken right = ctx.OwnerToken;

                    //Contains 方法分为 List.Contains 和 Enumerable.Contains 两种情况，前者是实例方法，后者是扩展方法
                    if (ctx.OwnerToken == null)
                    {
                        if (ctx.MethodParameterTokenList.Length != 2)
                            return; //无法将该Container方法翻译为 IN 操作，直接走默认流程

                        right = ctx.MethodParameterTokenList[0];
                        left = ctx.MethodParameterTokenList[1];
                    }
                    else
                    {
                        left = ctx.MethodParameterTokenList[0];
                    }

                    //拆包
                    if (right is SqlParameterToken p)
                    {
                        if (p.Value is ILTSQLObjectQueryable query)
                        {
                            // do nothing
                            right = p;
                        }
                        else if (p.Value is IEnumerable list)
                        {
                            //list 拆包
                            List<SqlParameterToken> paras = new List<SqlParameterToken>();
                            foreach (object item in list)
                                paras.Add(ctx.TokenSqlParameter(item));

                            right = LTSQLTokenFactory.CreatePriorityCalcToken(LTSQLTokenFactory.CreateListToken(paras.ToArray()));
                        }
                    }

                    ctx.ResultToken = LTSQLTokenFactory.CreateBoolCalcToken(BoolCalcToken.OPT_IN, left, right);
                }

                // in 操作，支持元组匹配
                else if (typeof(ExpressionFunctionExtensions) == ctx.OwnerType
                && ctx.Member.Name == nameof(ExpressionFunctionExtensions.In)
                && ctx.MethodParameterTokenList.Length == 2)
                {
                    TupleToken tuple = ctx.MethodParameterTokenList[0] as TupleToken;
                    LTSQLToken token = ctx.MethodParameterTokenList[1];
                    ILTSQLObjectQueryable query =  (token is SqlParameterToken p1) ? p1.Value as ILTSQLObjectQueryable : null;;
                    IEnumerable list = (token is SqlParameterToken p2) ? p2.Value as IEnumerable : null;
                    if(tuple == null)
                        throw new NotSupportedException("In操作符号进行元组匹配时，必须是元组。");
                    if(query == null && list == null)
                        throw new NotSupportedException("In操作符号进行元组匹配时，参数不正确，必须时子查询或者参数列表。");

                    //子查询
                    if (query != null)
                    {
                        ctx.ResultToken = LTSQLTokenFactory.CreateBoolCalcToken(BoolCalcToken.OPT_IN, tuple, token);
                    }
                    //参数硬编码
                    else if (list != null)
                    {
                        List<TupleToken> tokens = new List<TupleToken>();
                        foreach (object para in list)
                        {
                            Type t = para.GetType();
                            TupleToken tupleItem = new TupleToken(t);
                            foreach (string prop in tuple.PropNames)
                            {
                                MemberInfo[] members = t.GetMember(prop).Where(p => p is PropertyInfo || p is FieldInfo).ToArray();
                                if (members == null || members.Length <= 0)
                                    throw new Exception($"In操作符号进行元组匹配时，无法获取参数的属性信息或者字段信息，成员名称为：{prop}");
                                if (members.Length > 1)
                                    throw new Exception($"In操作符号进行元组匹配时，获取到了多个同名成员，成员名称为：{prop}");

                                MemberInfo member = members[0];
                                PropertyInfo memberProp = member as PropertyInfo;
                                FieldInfo memberField = member as FieldInfo;

                                //string propName = member.Name;
                                object propValue = memberProp?.GetValue(para) ?? memberField?.GetValue(para);
                                SqlParameterToken parameter = ctx.TokenSqlParameter(propValue, memberProp?.PropertyType ?? memberField?.FieldType);
                                tupleItem.Add(parameter, member.Name);
                            }
                            tokens.Add(tupleItem);
                        }
                        ctx.ResultToken = LTSQLTokenFactory.CreateBoolCalcToken(BoolCalcToken.OPT_IN, tuple,
                                LTSQLTokenFactory.CreatePriorityCalcToken(LTSQLTokenFactory.CreateListToken(tokens.ToArray()))
                            );
                    }
                }
            });


            // EXISTS 操作
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                if ((typeof(Enumerable) == ctx.OwnerType
                    || typeof(LTSQLQueryableExtensions) == ctx.OwnerType
                    || typeof(IEnumerable).IsAssignableFrom(ctx.OwnerType))
                    && (ctx.Member.Name == nameof(Enumerable.Any))
                    && ctx.MethodParameterTokenList.IsNotEmpty()
                    && ctx.MethodParameterTokenList.Length == 1
                    )
                {
                    LTSQLToken inner = ctx.MethodParameterTokenList[0];
                    SqlParameterToken p = inner as SqlParameterToken;

                    //拆包
                    if (p != null && p.Value is ILTSQLObjectQueryable query)
                    {
                        // do nothing
                        inner = p;
                    }
                    else if (p != null && p.Value is IEnumerable list)
                    {
                        //list 拆包
                        List<SqlParameterToken> paras = new List<SqlParameterToken>();
                        foreach (object item in list)
                            paras.Add(ctx.TokenSqlParameter(item));

                        //非法的，除非能够转换成 select 
                        inner = LTSQLTokenFactory.CreateListToken(paras.ToArray());
                    }

                    ctx.ResultToken = LTSQLTokenFactory.CreateBoolCalcToken(BoolCalcToken.OPT_EXISTS, null, inner);
                    //ctx.ResultToken = SqlFunctionHelper.ExistsFunction(ctx.Options.DbType, inner).Builder();
                }
            });


            // 对 FirstOrDefault 的支持(等同于Take(1)函数)
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                if (ctx.Member.Name == nameof(ExpressionFunctionExtensions.FirstOrDefault) && ctx.OwnerType == typeof(ExpressionFunctionExtensions))
                {
                    if (ctx.MethodParameterTokenList.IsEmpty() || ctx.MethodParameterTokenList.Length != 1)
                        return;

                    LTSQLToken token = ctx.MethodParameterTokenList[0];
                    if (token is SqlParameterToken p && p.Value is ILTSQLObjectQueryable query)
                    {
                        //调用 FirstOrDefault 之后需要调整参数
                        MethodInfo method = ctx.Member as MethodInfo;
                        method.Invoke(null, new[] { query }); //直接调用静态方法：LTSQLQueryableExtensions.FirstOrDefault, 其内部会做相关处理
                        ctx.ResultToken = LTSQLTokenFactory.CreateSqlParameterToken(p.ParameterName, query, method.ReturnType);
                    }
                }
            });

            InitForString(defaultTranslater);
            InitForDatetime(defaultTranslater);
            return defaultTranslater;
        }

        private static LTSQLTokenTranslaterSelector InitForString(LTSQLTokenTranslaterSelector defaultTranslater)
        {
            // 字符串 Length 函数
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                if (ctx.OwnerType == typeof(string) && ctx.Member.Name == nameof(string.Length))
                {
                    if (ctx.Options?.DbType == DbType.MSSQL)
                        ctx.ResultToken = LTSQLTokenFactory.CreateFunctionCallToken("LEN", new[] { ctx.OwnerToken }, typeof(int));
                    else if (ctx.Options?.DbType == DbType.MySQL)
                        ctx.ResultToken = LTSQLTokenFactory.CreateFunctionCallToken("CHAR_LENGTH", new[] { ctx.OwnerToken }, typeof(int));
                    else
                        ctx.ResultToken = LTSQLTokenFactory.CreateFunctionCallToken("LENGTH", new[] { ctx.OwnerToken }, typeof(int));
                }
            });


            // 字符串拼接函数
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                if (ctx.OwnerType == typeof(string) && ctx.Member.Name == nameof(string.Concat))
                {
                    DbType db = ctx.Options.DbType;
                    if (db == DbType.SQLLite || db == DbType.MSSQL || db == DbType.PGSQL)
                        ctx.ResultToken = LTSQLTokenFactory.CreateFunctionCallToken("CONCAT", ctx.MethodParameterTokenList, typeof(string));
                    else if (db == DbType.MySQL)
                        ctx.ResultToken = LTSQLTokenFactory.CreateFunctionCallToken("CONCAT_WS", new[] { LTSQLTokenFactory.CreateConstantToken("", db, typeof(string)) }.Concat(ctx.MethodParameterTokenList).ToArray(), typeof(string));
                    else if (db == DbType.Oracle)
                    {
                        LTSQLToken concat = null;
                        foreach (var token in ctx.MethodParameterTokenList)
                        {
                            if (concat == null)
                                concat = token;
                            else
                                concat = new BinaryToken("||", concat, token, typeof(string));
                        }

                        ctx.ResultToken = concat != null ? LTSQLTokenFactory.CreatePriorityCalcToken(concat as SqlValueToken) : null;
                    }
                }
            });


            // 字符串截取
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                if (ctx.OwnerType == typeof(string) && ctx.Member.Name == nameof(string.Substring))
                {
                    DbType db = ctx.Options.DbType;
                    if (db == DbType.Oracle || db == DbType.SQLLite)
                    {
                        ctx.ResultToken = LTSQLTokenFactory.CreateFunctionCallToken("SUBSTR", new[] { ctx.OwnerToken }.Concat(ctx.MethodParameterTokenList).ToArray(), typeof(string));
                    }
                    else if (db == DbType.MySQL || db == DbType.MSSQL || db == DbType.PGSQL)
                    {
                        ctx.ResultToken = LTSQLTokenFactory.CreateFunctionCallToken("SUBSTRING", new[] { ctx.OwnerToken }.Concat(ctx.MethodParameterTokenList).ToArray(), typeof(string));
                    }
                }
            });


            // 字符串前后空格去除
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                if (ctx.OwnerType == typeof(string) && ctx.Member.Name == nameof(string.TrimStart))
                {
                    ctx.ResultToken = LTSQLTokenFactory.CreateFunctionCallToken("LTRIM", new[] { ctx.OwnerToken }, typeof(string));
                }
                else if (ctx.OwnerType == typeof(string) && ctx.Member.Name == nameof(string.TrimEnd))
                {
                    ctx.ResultToken = LTSQLTokenFactory.CreateFunctionCallToken("RTRIM", new[] { ctx.OwnerToken }, typeof(string));
                }
                else if (ctx.OwnerType == typeof(string) && ctx.Member.Name == nameof(string.Trim))
                {
                    LTSQLToken trimL = LTSQLTokenFactory.CreateFunctionCallToken("LTRIM", new[] { ctx.OwnerToken }, typeof(string));
                    ctx.ResultToken = LTSQLTokenFactory.CreateFunctionCallToken("RTRIM", new[] { trimL }, typeof(string));
                }
            });


            //字符串匹配：Contains / startWith / endWith
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                MethodInfo strMethod = ctx.Member as MethodInfo;
                if (strMethod == null || strMethod.GetParameters().Length != 1)
                    return;

                // liek %xxx%
                if (ctx.OwnerType == typeof(string) && ctx.Member.Name == nameof(string.Contains))
                {
                    LTSQLToken concat1 = LTSQLTokenFactory.CreateFunctionCallToken("CONCAT", new[] {
                        LTSQLTokenFactory.CreateConstantToken('%', ctx.Options.DbType),
                        ctx.MethodParameterTokenList[0]
                    }, typeof(string));

                    LTSQLToken concat2 = LTSQLTokenFactory.CreateFunctionCallToken("CONCAT", new LTSQLToken[] {
                        concat1,
                        LTSQLTokenFactory.CreateConstantToken('%', ctx.Options.DbType)
                    }, typeof(string));

                    ctx.ResultToken = LTSQLTokenFactory.CreateBoolCalcToken(BoolCalcToken.OPT_LIKE, ctx.OwnerToken, concat2);
                }
                //like xxx%
                else if (ctx.OwnerType == typeof(string) && ctx.Member.Name == nameof(string.StartsWith))
                {
                    LTSQLToken concat1 = LTSQLTokenFactory.CreateFunctionCallToken("CONCAT", new[] {
                        ctx.MethodParameterTokenList[0],
                        LTSQLTokenFactory.CreateConstantToken('%', ctx.Options.DbType)
                    }, typeof(string));

                    ctx.ResultToken = LTSQLTokenFactory.CreateBoolCalcToken(BoolCalcToken.OPT_LIKE, ctx.OwnerToken, concat1);
                }
                //like xxx%
                else if (ctx.OwnerType == typeof(string) && ctx.Member.Name == nameof(string.EndsWith))
                {
                    LTSQLToken concat1 = LTSQLTokenFactory.CreateFunctionCallToken("CONCAT", new[] {
                        LTSQLTokenFactory.CreateConstantToken('%', ctx.Options.DbType),
                        ctx.MethodParameterTokenList[0]
                    }, typeof(string));

                    ctx.ResultToken = LTSQLTokenFactory.CreateBoolCalcToken(BoolCalcToken.OPT_LIKE, ctx.OwnerToken, concat1);
                }
            });


            return defaultTranslater;
        }

        private static LTSQLTokenTranslaterSelector InitForDatetime(LTSQLTokenTranslaterSelector defaultTranslater)
        {
            // 日期时间函数：Year / Month / Day / Hour / Minute / Second / ToString
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                if (ctx.OwnerType != typeof(DateTime))
                    return;

                DbType db = ctx.Options.DbType;
                if (ctx.Member.Name == nameof(DateTime.Year))
                {
                    ctx.ResultToken = SqlFunctionHelper.DateYearFunction(db, ctx.OwnerToken).Builder();
                }
                else if (ctx.Member.Name == nameof(DateTime.Month))
                {
                    ctx.ResultToken = SqlFunctionHelper.DateMonthFunction(db, ctx.OwnerToken).Builder();
                }
                else if (ctx.Member.Name == nameof(DateTime.Day))
                {
                    ctx.ResultToken = SqlFunctionHelper.DateDayFunction(db, ctx.OwnerToken).Builder();
                }
                else if (ctx.Member.Name == nameof(DateTime.Hour))
                {
                    ctx.ResultToken = SqlFunctionHelper.DateHourFunction(db, ctx.OwnerToken).Builder();
                }
                else if (ctx.Member.Name == nameof(DateTime.Minute))
                {
                    ctx.ResultToken = SqlFunctionHelper.DateMinuteFunction(db, ctx.OwnerToken).Builder();
                }
                else if (ctx.Member.Name == nameof(DateTime.Second))
                {
                    ctx.ResultToken = SqlFunctionHelper.DateSecondFunction(db, ctx.OwnerToken).Builder();
                }
                else if (ctx.Member.Name == nameof(DateTime.ToString) && ctx.Member is MethodInfo method && method.GetParameters().Length == 1)
                {
                    ctx.ResultToken = SqlFunctionHelper.DateFormatFunction(db, ctx.OwnerToken, ctx.MethodParameterTokenList[0]).Builder();
                }
            });

            return defaultTranslater;
        }
    }
}