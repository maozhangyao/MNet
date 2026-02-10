using MNet.LTSQL.v1.SqlTokens;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace MNet.LTSQL.v1
{
    /// <summary>
    /// 
    /// </summary>
    public class TranslateContext
    {
        public TranslateContext()
        { }
        public TranslateContext(Stack<LTSQLToken> tokens)
        {
            this.Tokens = tokens;
        }


        internal Stack<LTSQLToken> Tokens;
        public LTSQLOptions Options { get; set; }
        public NameGenerator ParameterNameGenerator { get; set; }
        

        public Expression TranslateExpr { get; set; }
        public LTSQLToken ResultToken { get; set; }
        public MemberInfo TranslateMember { get; set; }
        //对于调用静态成员，该属性为nul
        public object? MemberOwner { get; set; }
        public Type MemberOnwerType { get; set; }
        

        public LTSQLToken PopToken()
        {
            return this.Tokens == null || this.Tokens.Count < 1 ? null : this.Tokens.Pop();
        }
        public LTSQLToken PeekToken()
        {
            return this.Tokens?.Peek();
        }
        public LTSQLToken[] PopParameters(int argsCnt)
        {
            if (argsCnt < 1)
                throw new Exception($"无效参数{nameof(argsCnt)}");
            if (this.Tokens == null || argsCnt > this.Tokens.Count)
                throw new Exception($"参数{nameof(argsCnt)}:{argsCnt} 值超过实际token数量");

            Stack<LTSQLToken> args = new Stack<LTSQLToken>(argsCnt);
            for (int i = 0; i < argsCnt; i++)
                args.Push(this.Tokens.Pop());

            return args.ToArray();
        }
        public SqlParameterToken TokenSqlParameter(object value)
        {
            return new SqlParameterToken()
            {
                Value = value,
                ParameterName = this.ParameterNameGenerator.Next()
            };
        }
        internal void ClearProps()
        {
            this.Tokens = null;
            this.Options = null;
            this.ParameterNameGenerator = null;
            this.TranslateExpr = null;
            this.ResultToken = null;
            this.TranslateMember = null;
            this.MemberOwner = null;
        }
    }
}