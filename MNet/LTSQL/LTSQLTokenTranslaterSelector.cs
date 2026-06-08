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
using MNet.LTSQL.SqlQueryStructs;

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
                MethodInfo mthd = ctx.Member as MethodInfo;
                if (ctx.TranslateExpr is MethodCallExpression callExpr)
                {
                    if (ctx.MethodParameterTokenList.IsEmpty())
                        return;
                    if (mthd == null || ctx.Owner != null)
                        return;
                    if (ctx.OwnerType != typeof(Enumerable) && ctx.OwnerType != typeof(LTSQLQueryableExtensions))
                        return;

                    string sqlFunc = "";
                    string extdName = "";
                    string mthdName = ctx.Member.Name;
                    string sum = nameof(Enumerable.Sum);
                    string max = nameof(Enumerable.Max);
                    string min = nameof(Enumerable.Min);
                    string avg = nameof(Enumerable.Average);
                    string cnt = nameof(Enumerable.Count);
                    string lcnt = nameof(Enumerable.LongCount);

                    bool flag = true;
                    if (mthdName == sum)
                    {
                        sqlFunc = "SUM";
                        extdName = nameof(LTSQLQueryableExtensions.WithSum);
                    }
                    else if (mthdName == max)
                    {
                        sqlFunc = "MAX";
                        extdName = nameof(LTSQLQueryableExtensions.WithMax);
                    }
                    else if (mthdName == min)
                    {
                        sqlFunc = "MIN";
                        extdName = nameof(LTSQLQueryableExtensions.WithMin);
                    }
                    else if (mthdName == avg)
                    {
                        sqlFunc = "AVG";
                        extdName = nameof(LTSQLQueryableExtensions.WithAverage);
                    }
                    else if (mthdName == cnt)
                    {
                        sqlFunc = "COUNT";
                        extdName = nameof(LTSQLQueryableExtensions.WithCount);
                    }
                    else if (mthdName == lcnt)
                    {
                        sqlFunc = "COUNT";
                        extdName = nameof(LTSQLQueryableExtensions.WithLongCount);
                    }
                    else
                    {
                        flag = false;
                    }

                    if (!flag)
                        return;

                    // SUM 是扩展方法，所以该方法的第一个参数表示实例对象
                    LTSQLToken inst = ctx.MethodParameterTokenList[0];
                    if (inst == null)
                        return;

                    if (inst is GroupObjToken gpObj)
                    {
                        LTSQLToken[] parameters = ctx.MethodParameterTokenList.Skip(1).ToArray();
                        if((mthdName == cnt || mthdName == lcnt) && parameters.IsEmpty())
                            parameters = new[] { SyntaxToken.Create("*") };

                        //ctx.ResultToken = LTSQLTokenFactory.CreateFunctionCallToken(sqlFunc, parameters, mthd.ReturnType);
                        ctx.ResultToken = new FunctionTokenBuilder().WithFunctionName(sqlFunc, mthd.ReturnType).WithFunctionArgs(parameters).Build();
                    }
                    else if (inst.TryGetSqlQueryable(out ILTSQLObjectQueryable query))
                    {
                        ILTSQLObjectQueryable newQuery = (ILTSQLObjectQueryable)InvokeCommonGroupMethod(extdName, callExpr, query);
                        ctx.ResultToken = ctx.TokenSqlParameter(newQuery);
                    }
                }
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
                    && (ctx.Member.Name == nameof(Enumerable.Contains))
                )
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

                            right = LTSQLTokenFactory.CreateListToken(paras.ToArray()).SetPriority(true) as ListToken;
                        }
                    }

                    ctx.ResultToken = LTSQLTokenFactory.CreateBoolCalcToken(BoolCalcToken.OPT_IN, left, right.TryPriority(true));
                }

                // in 操作，支持元组匹配
                else if (typeof(ExpressionFunctionExtensions) == ctx.OwnerType
                && ctx.Member.Name == nameof(ExpressionFunctionExtensions.In)
                && ctx.MethodParameterTokenList.Length == 2
                )
                {
                    TupleToken tuple = ctx.MethodParameterTokenList[0] as TupleToken;
                    LTSQLToken token = ctx.MethodParameterTokenList[1];
                    ILTSQLObjectQueryable query = (token is SqlParameterToken p1) ? p1.Value as ILTSQLObjectQueryable : null;
                    IEnumerable list = (token is SqlParameterToken p2) ? p2.Value as IEnumerable : null;
                    if (tuple == null)
                        throw new NotSupportedException("In操作符号进行元组匹配时，必须是元组。");
                    if(query == null && list == null)
                        throw new NotSupportedException("In操作符号进行元组匹配时，参数不正确，必须时子查询或者参数列表。");

                    //子查询
                    if (query != null)
                    {
                        ctx.ResultToken = LTSQLTokenFactory.CreateBoolCalcToken(BoolCalcToken.OPT_IN, tuple, token, true);
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
                                tupleItem.Add(member.Name, parameter, parameter.ValueType);
                            }
                            tokens.Add(tupleItem);
                        }
                        ctx.ResultToken = LTSQLTokenFactory.CreateBoolCalcToken(BoolCalcToken.OPT_IN, tuple,
                                LTSQLTokenFactory.CreateListToken(true, tokens.ToArray()), true
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

                    ctx.ResultToken = SqlFunctionHelper.ExistsFunction(ctx.Options.DbType, inner.TryPriority(false)).Build();
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
                        MethodInfo firstOrDefaultMthd = ctx.Member as MethodInfo;
                        MethodInfo takeMthd = typeof(LTSQLQueryableExtensions).GetMethod(nameof(LTSQLQueryableExtensions.Take))
                            .MakeGenericMethod(firstOrDefaultMthd.GetGenericArguments()[0]);

                        // Take(1)
                        ILTSQLObjectQueryable newQuery = (ILTSQLObjectQueryable)takeMthd.Invoke(null, new object[] { query, 1 });
                        //调用FirstOrDefault方法之后，参数类型需要调整为FirstOrDefault方法的返回值
                        ctx.ResultToken = ctx.TokenSqlParameter(newQuery, firstOrDefaultMthd.ReturnType);
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
                    ctx.ResultToken = SqlFunctionHelper.StringLengthFunction(ctx.Options.DbType, ctx.OwnerToken).Build();
            });


            // 字符串拼接函数
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                if (ctx.OwnerType == typeof(string) && ctx.Member.Name == nameof(string.Concat))
                {
                    DbTypes db = ctx.Options.DbType;
                    ctx.ResultToken = SqlFunctionHelper.StringConcatFunction(db, ctx.MethodParameterTokenList).Build();
                }
            });


            // 字符串截取
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                if (ctx.OwnerType == typeof(string) && ctx.Member.Name == nameof(string.Substring))
                {
                    DbTypes db = ctx.Options.DbType;
                    ctx.ResultToken = SqlFunctionHelper.StringSubstrFunction(db, ctx.OwnerToken, ctx.MethodParameterTokenList[0], ctx.MethodParameterTokenList[1]).Build();
                }
            });


            // 字符串前后空格去除
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                DbTypes db = ctx.Options.DbType;
                if (ctx.OwnerType == typeof(string) && ctx.Member.Name == nameof(string.TrimStart))
                {
                    ctx.ResultToken = SqlFunctionHelper.StringTrimLFunction(db, ctx.OwnerToken).Build();
                }
                else if (ctx.OwnerType == typeof(string) && ctx.Member.Name == nameof(string.TrimEnd))
                {
                    ctx.ResultToken = SqlFunctionHelper.StringTrimRFunction(db, ctx.OwnerToken).Build();
                }
                else if (ctx.OwnerType == typeof(string) && ctx.Member.Name == nameof(string.Trim))
                {
                    ctx.ResultToken = SqlFunctionHelper.StringTrimFunction(db, ctx.OwnerToken).Build();
                }
            });


            //字符串匹配：Contains / startWith / endWith
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                MethodInfo strMethod = ctx.Member as MethodInfo;
                if (strMethod == null || strMethod.GetParameters().Length != 1)
                    return;

                DbTypes db = ctx.Options.DbType;
                // liek %xxx%
                if (ctx.OwnerType == typeof(string) && ctx.Member.Name == nameof(string.Contains))
                {
                    LTSQLToken liekStrToken = SqlFunctionHelper.StringLikeConcat(db, ctx.MethodParameterTokenList[0]).Build();
                    ctx.ResultToken = LTSQLTokenFactory.CreateBoolCalcToken(BoolCalcToken.OPT_LIKE, SqlFunctionHelper.StringLikeLConcat(db, ctx.OwnerToken).Build(), liekStrToken);
                }
                //like xxx%
                else if (ctx.OwnerType == typeof(string) && ctx.Member.Name == nameof(string.StartsWith))
                {
                    LTSQLToken liekStrToken = SqlFunctionHelper.StringLikeLConcat(db, ctx.MethodParameterTokenList[0]).Build();
                    ctx.ResultToken = LTSQLTokenFactory.CreateBoolCalcToken(BoolCalcToken.OPT_LIKE, ctx.OwnerToken, liekStrToken);
                }
                //like xxx%
                else if (ctx.OwnerType == typeof(string) && ctx.Member.Name == nameof(string.EndsWith))
                {
                    LTSQLToken liekStrToken = SqlFunctionHelper.StringLikeRConcat(db, ctx.MethodParameterTokenList[0]).Build();
                    ctx.ResultToken = LTSQLTokenFactory.CreateBoolCalcToken(BoolCalcToken.OPT_LIKE, ctx.OwnerToken, liekStrToken);
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

                DbTypes db = ctx.Options.DbType;
                if (ctx.Member.Name == nameof(DateTime.Year))
                {
                    ctx.ResultToken = SqlFunctionHelper.DateYearFunction(db, ctx.OwnerToken).Build();
                }
                else if (ctx.Member.Name == nameof(DateTime.Month))
                {
                    ctx.ResultToken = SqlFunctionHelper.DateMonthFunction(db, ctx.OwnerToken).Build();
                }
                else if (ctx.Member.Name == nameof(DateTime.Day))
                {
                    ctx.ResultToken = SqlFunctionHelper.DateDayFunction(db, ctx.OwnerToken).Build();
                }
                else if (ctx.Member.Name == nameof(DateTime.Hour))
                {
                    ctx.ResultToken = SqlFunctionHelper.DateHourFunction(db, ctx.OwnerToken).Build();
                }
                else if (ctx.Member.Name == nameof(DateTime.Minute))
                {
                    ctx.ResultToken = SqlFunctionHelper.DateMinuteFunction(db, ctx.OwnerToken).Build();
                }
                else if (ctx.Member.Name == nameof(DateTime.Second))
                {
                    ctx.ResultToken = SqlFunctionHelper.DateSecondFunction(db, ctx.OwnerToken).Build();
                }
                else if (ctx.Member.Name == nameof(DateTime.ToString) && ctx.Member is MethodInfo method && method.GetParameters().Length == 1)
                {
                    ctx.ResultToken = SqlFunctionHelper.DateFormatFunction(db, ctx.OwnerToken, ctx.MethodParameterTokenList[0]).Build();
                }
            });

            return defaultTranslater;
        }


        //通用分组方法调用
        private static object InvokeCommonGroupMethod(string gpMethod, MethodCallExpression callExpr, ILTSQLObjectQueryable qInst)
        {
            int argsLen = callExpr.Arguments.Count;

            Expression aggExpr = argsLen < 2 ? null : callExpr.Arguments[1]; // 聚合的lambda
            Type selectorValueType = argsLen < 2 ? null : aggExpr.AsLambda().ReturnType; // 需要聚合的类型
            Type entityType = callExpr.Method.GetGenericArguments()[0]; //实体类型
            Type aggRetType = callExpr.Method.ReturnType;// 聚合之后的值的类型

            MethodInfo m = GetExtMethod(gpMethod, m =>
            {
                var paras = m.GetParameters();
                if (paras.Length != argsLen)
                    return false;

                //扩展方法：所以至少有一个参数有的
                if (argsLen < 2)
                    return m.ReturnType.GetGenericArguments()[0] == aggRetType;

                //聚合值的类型(注意：聚合值和聚合后的值类型不一定一样)
                Type t = paras[1].ParameterType.GetGenericArguments()[0].GetGenericArguments()[1];
                return m.ReturnType.GetGenericArguments()[0] == aggRetType && t == selectorValueType;
            });

            if (argsLen == 1)
                return InvokeExtMethod(m, new[] { entityType }, qInst);
            return InvokeExtMethod(m, new[] { entityType }, qInst, aggExpr);
        }
        private static MethodInfo GetExtMethod(string gpMethod, Func<MethodInfo, bool> where)
        {
            MethodInfo[] ms = typeof(LTSQLQueryableExtensions).GetMethods()
                .Where(p => p.Name == gpMethod)
                .Where(p => where(p))
                .ToArray();

            if (ms.IsEmpty())
                throw new Exception($"{gpMethod}方法未找到。");
            if (ms.Length != 1)
                throw new Exception($"{gpMethod}方法匹配到多个无法确定唯一。");

            return ms[0];
        }
        private static object InvokeExtMethod(MethodInfo mthd, Type[] makeTypes, params object[] args)
        {
            if (makeTypes.IsNotEmpty())
            {
                mthd = mthd.MakeGenericMethod(makeTypes);
            }
            return mthd.Invoke(null, args);
        }
    }
}