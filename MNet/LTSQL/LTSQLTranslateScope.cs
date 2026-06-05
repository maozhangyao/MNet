using System;

namespace MNet.LTSQL
{
    //sql 作用域
    public class LTSQLTranslateScope
    {
        public LTSQLTranslateScope(LTSQLContext context)
        {
            if(context == null)
                throw new ArgumentNullException(nameof(context));
                
            this.Context = context;
            this.ScopeId = Guid.NewGuid().ToString("N");
        }

        public string ScopeId { get; private set; }
        public LTSQLContext Context { get; }
        public LTSQLTranslateScope Parent { get; private set; }
        

        public LTSQLTranslateScope NewScope()
        {
            var ctx = new LTSQLContext() { 
                    Options = this.Context?.Options,
                    TableAliasGenerator = this.Context?.TableAliasGenerator,
                    ParameterNameGenerator = this.Context?.ParameterNameGenerator,
                    LTSQLTranslater = this.Context?.LTSQLTranslater,
                };
            return new LTSQLTranslateScope(ctx)
            {
                Parent = this
            };
        }
    }
}
