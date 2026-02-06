using System;
using System.Collections.Generic;
using MNet.LTSQL.v1.SqlTokens;
using System.Reflection;
using System.Linq.Expressions;

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
    }

    internal class CombineTranslaterSelector : LTSQLTokenTranslaterSelector
    {
        public CombineTranslaterSelector(LTSQLTokenTranslaterSelector high, LTSQLTokenTranslaterSelector low)
        {
            this._low = low;
            this._high = high;
        }

        private LTSQLTokenTranslaterSelector _low;
        private LTSQLTokenTranslaterSelector _high;


        public override void TranslateMember(TranslateContext context)
        {
            this._high?.TranslateMember(context);
            if (context.ResultToken == null)
                this._low?.TranslateMember(context);
        }
        public override void TranslateExpression(TranslateContext context)
        {
            this._high?.TranslateExpression(context);
            if (context.ResultToken == null)
                this._low?.TranslateExpression(context);
        }
    }
}