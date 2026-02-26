using System;

namespace MNet.LTSQL.v1
{
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