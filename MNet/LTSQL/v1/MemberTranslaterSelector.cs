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
    public class MemberTranslaterSelector
    {
        public MemberTranslaterSelector()
        {
            this._translates = new List<(Func<Type, MemberInfo, bool>, Action<TranslateContext>)>(16);
        }


        private List<(Func<Type, MemberInfo, bool>, Action<TranslateContext>)> _translates;


        public Action<TranslateContext> Select(Type mappingType, MemberInfo member)
        {
            foreach (var (when, translater) in this._translates)
            {
                if (when(mappingType, member))
                    return translater;
            }
            return null;
        }
        public MemberTranslaterSelector Use(Func<Type, MemberInfo, bool> when, Action<TranslateContext> translater)
        {
            if (when == null || translater == null)
                throw new ArgumentNullException(nameof(when) + ", " + nameof(translater));

            this._translates.Add((when, translater));
            return this;
        }
    }
}