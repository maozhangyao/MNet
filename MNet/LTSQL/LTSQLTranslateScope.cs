using System;

namespace MNet.LTSQL
{
    //sql 作用域
    public class LTSQLTranslateScope
    {
        public LTSQLTranslateScope()
        {
            this.ScopeId = Guid.NewGuid().ToString("N");
        }
        public LTSQLTranslateScope(LTSQLContext context) : this()
        {
            if(context == null)
                throw new ArgumentNullException(nameof(context));
                
            this.Context = context;
        }

        public string ScopeId { get; private set; }
        public LTSQLContext Context { get; private set; }
        public LTSQLTranslateScope Parent { get; private set; }

        public LTSQLTranslateScope NewScope()
        {
            var ctx = new LTSQLContext(this.Context?.Options)
            {
                TableAliasGenerator = this.Context?.TableAliasGenerator,
                ParameterNameGenerator = this.Context?.ParameterNameGenerator,
                LTSQLTranslater = this.Context?.LTSQLTranslater,
            };
            return new LTSQLTranslateScope(ctx)
            {
                Parent = this
            };
        }
        public void SetContext(LTSQLContext context) 
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            this.Context = context;
        }
    }
}
