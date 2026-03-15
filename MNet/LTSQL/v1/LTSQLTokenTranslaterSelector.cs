using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MNet.LTSQL.v1.SqlTokens;
using System.Reflection;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MNet.Utils;

namespace MNet.LTSQL.v1
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
            if(translate != null)
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

            foreach(Action<TranslateContext> translater in this._memberTranslaters)
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
                    ctx.ResultToken = new FunctionToken("SUM", parameters, sumMethod.ReturnType);
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
                    ctx.ResultToken = new FunctionToken("MAX", parameters, maxMethod.ReturnType);
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
                    ctx.ResultToken = new FunctionToken("MIN", parameters, minMethod.ReturnType);
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
                    ctx.ResultToken = new FunctionToken("AVG", parameters, avgMethod.ReturnType);
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
                    ctx.ResultToken = new FunctionToken("COUNT", (parameters.IsEmpty() ? new [] { new ConstantToken("*") } : parameters), cntMethod.ReturnType);
            });


            // IN 操作
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                if ((typeof(Enumerable) == ctx.OwnerType 
                    || typeof(LTSQLQueryableExtensions) == ctx.OwnerType 
                    || typeof(IEnumerable).IsAssignableFrom(ctx.OwnerType)) 
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

                            right = new SqlScopeToken(new TokenItemListToken(paras));
                        }
                    }

                    ctx.ResultToken = new ConditionToken(left, right, ConditionToken.OPT_IN);
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

                        inner = new SqlScopeToken(new TokenItemListToken(paras));
                    }

                    ctx.ResultToken = new ConditionToken(null, inner, ConditionToken.OPT_EXISTS);
                }
            });


            // 对 FirstOrDefault 的支持(等同于Take(1)函数)
            defaultTranslater.UseMemberTranslate(ctx => {
                if (ctx.Member.Name == nameof(LTSQLQueryableExtensions.FirstOrDefault) && ctx.OwnerType == typeof(LTSQLQueryableExtensions))
                {
                    if (ctx.MethodParameterTokenList.IsEmpty() || ctx.MethodParameterTokenList.Length != 1)
                        return;

                    LTSQLToken token = ctx.MethodParameterTokenList[0];
                    if (token is SqlParameterToken p && p.Value is ILTSQLObjectQueryable query)
                    {
                        //调用 FirstOrDefault 之后需要调整参数
                        MethodInfo method = ctx.Member as MethodInfo;
                        method.Invoke(null, new[] { query }); //直接调用静态方法：LTSQLQueryableExtensions.FirstOrDefault, 其内部会做相关处理
                        ctx.ResultToken = new SqlParameterToken(p.ParameterName, query, method.ReturnType);
                    }
                }
            });            


            // 字符串 Length 函数
            defaultTranslater.UseMemberTranslate(ctx => {
                if (ctx.OwnerType == typeof(string) && ctx.Member.Name == nameof(string.Length))
                {
                    if (ctx.Options?.DbType == DbType.MSSQL)
                        ctx.ResultToken = new FunctionToken("LEN", new[] { ctx.OwnerToken }, typeof(int));
                    else if (ctx.Options?.DbType == DbType.MySQL)
                        ctx.ResultToken = new FunctionToken("CHAR_LENGTH", new[] { ctx.OwnerToken }, typeof(int));
                    else
                        ctx.ResultToken = new FunctionToken("LENGTH", new[] { ctx.OwnerToken }, typeof(int));
                }
            });


            return defaultTranslater;
        }
    }
}