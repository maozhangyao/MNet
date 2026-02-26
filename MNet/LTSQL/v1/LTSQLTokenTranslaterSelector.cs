using System;
using System.Linq;
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
        static LTSQLTokenTranslaterSelector()
        {
            InitDefault();
        }


        public LTSQLTokenTranslaterSelector()
        {
            this._memberTranslaters = new List<Action<TranslateContext>>();
            this._expressionTranslaters = new List<Action<TranslateContext>>();
        }


        private List<Action<TranslateContext>> _memberTranslaters;
        private List<Action<TranslateContext>> _expressionTranslaters;


        public static readonly LTSQLTokenTranslaterSelector Default = new LTSQLTokenTranslaterSelector();

        
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


        private static void InitDefault()
        {
            LTSQLTokenTranslaterSelector defaultTranslater = Default;

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
                    ctx.ResultToken = new FunctionToken("SUM", parameters);
            });


            // 聚合函数 MAX
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                MethodInfo sumMethod = ctx.Member as MethodInfo;
                if (sumMethod == null || ctx.Owner != null || ctx.Member.Name != nameof(Enumerable.Max))
                    return;
                if (ctx.OwnerType != typeof(Enumerable) && ctx.OwnerType != typeof(LTSQLQueryableExtensions))
                    return;
                if (ctx.MethodParameterTokenList.IsEmpty())
                    return;

                // MAX 是扩展方法，所以该方法的第一个参数表示实例对象
                GroupObjToken groupObj = ctx.MethodParameterTokenList[0] as GroupObjToken;
                LTSQLToken[] parameters = ctx.MethodParameterTokenList.Skip(1).ToArray();
                if (groupObj != null)
                    ctx.ResultToken = new FunctionToken("MAX", parameters);
            });


            // 聚合函数 MIN
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                MethodInfo sumMethod = ctx.Member as MethodInfo;
                if (sumMethod == null || ctx.Owner != null || ctx.Member.Name != nameof(Enumerable.Min))
                    return;
                if (ctx.OwnerType != typeof(Enumerable) && ctx.OwnerType != typeof(LTSQLQueryableExtensions))
                    return;
                if (ctx.MethodParameterTokenList.IsEmpty())
                    return;

                // MIN 是扩展方法，所以该方法的第一个参数表示实例对象
                GroupObjToken groupObj = ctx.MethodParameterTokenList[0] as GroupObjToken;
                LTSQLToken[] parameters = ctx.MethodParameterTokenList.Skip(1).ToArray();
                if (groupObj != null)
                    ctx.ResultToken = new FunctionToken("MIN", parameters);
            });


            // 聚合函数 COUNT
            defaultTranslater.UseMemberTranslate(ctx =>
            {
                MethodInfo sumMethod = ctx.Member as MethodInfo;
                if (sumMethod == null || ctx.Owner != null || (ctx.Member.Name != nameof(Enumerable.Count) && ctx.Member.Name != nameof(Enumerable.LongCount)))
                    return;
                if (ctx.OwnerType != typeof(Enumerable) && ctx.OwnerType != typeof(LTSQLQueryableExtensions))
                    return;
                if (ctx.MethodParameterTokenList.IsEmpty())
                    return;

                // COUNT 是扩展方法，所以该方法的第一个参数表示实例对象
                GroupObjToken groupObj = ctx.MethodParameterTokenList[0] as GroupObjToken;
                LTSQLToken[] parameters = ctx.MethodParameterTokenList.Skip(1).ToArray();
                if (groupObj != null)
                    ctx.ResultToken = new FunctionToken("COUNT", (parameters.IsEmpty() ? new LTSQLToken[] { new ConstantToken("*") } : parameters));
            });
        }
    }
}