using MNet.LTSQL.SqlTokens;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace MNet.LTSQL
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


        //表达式所求的值的类型
        public Type ExpressionValueType { get; set; }
        public Expression TranslateExpr { get; set; }


        //成员(字段/属性/方法)
        public MemberInfo Member { get; set; }
        //成员所在的实例，对于调用静态成员，该属性为nul
        public object? Owner { get; set; }
        //成员所在的实例的类型
        public Type OwnerType { get; set; }
        //成员所在的实例对应的token结果
        public LTSQLToken OwnerToken { get; set; }
        //调用成员所需的参数token列表
        public LTSQLToken[] MethodParameterTokenList { get; set; }
        public LTSQLToken ResultToken { get; set; }


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
        public SqlParameterToken TokenSqlParameter(object value, Type type = null)
        {
            return LTSQLTokenFactory.CreateSqlParameterToken(this.ParameterNameGenerator.Next(), value, type ?? value.GetType());
        }
        internal void ClearProps()
        {
            this.Tokens = null;
            this.Options = null;
            this.ParameterNameGenerator = null;

            this.TranslateExpr = null;
            this.Member = null;
            this.Owner = null;
            this.OwnerType = null;
            this.OwnerToken = null;
            this.MethodParameterTokenList = null;
            this.ResultToken = null;
        }
    }
}