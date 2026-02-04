using System;
using MNet.LTSQL.v1.SqlTokens;
using System.Reflection;
using System.Linq.Expressions;

namespace MNet.LTSQL.v1
{
    //
    public class TranslateContext
    {
        public LTSQLOptions Options { get; set; }
        public Type TranslateType { get; set; }
        public MemberInfo TranslateMember { get; set; }
        public Expression TranslateExpr { get; set; }
        public LTSQLToken ResultToken { get; set; }
    }
}