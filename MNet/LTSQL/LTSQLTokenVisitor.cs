using MNet.LTSQL.SqlTokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL
{
    public class LTSQLTokenVisitor
    {
        public LTSQLTokenVisitor()
        {
            _visitor = token => token.VisitChildren(this);
        }

        private Func<LTSQLToken, LTSQLToken> _visitor;


        /// <summary>
        /// 遍历token及其子token，并对每个token调用visitor方法, 遍历顺序：先子后父。如果visitor方法返回非null值，则使用该值替换当前token。
        /// </summary>
        /// <param name="token"></param>
        /// <param name="visitor"></param>
        /// <returns></returns>
        public static LTSQLToken Visit(LTSQLToken token, Func<LTSQLToken, LTSQLToken> visitor)
        {
            LTSQLTokenVisitor v = new LTSQLTokenVisitor();
            v._visitor = t =>
            {
                var slef = t.VisitChildren(v);
                return visitor(slef ?? t);
            };

            return token.Visit(v);
        }
        /// <summary>
        /// 遍历token及其子token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="visitor">(当前token，继续访问子token的委托)</param>
        /// <returns></returns>
        public static LTSQLToken Visit(LTSQLToken token, Func<LTSQLToken, Func<LTSQLToken, LTSQLToken>, LTSQLToken> visitor)
        {
            LTSQLTokenVisitor v = new LTSQLTokenVisitor();
            v._visitor = t =>
            {
                return visitor(t, t2 => t2.VisitChildren(v));
            };

            return token.Visit(v);
        }


        public LTSQLToken Visit(LTSQLToken token)
        {
            return token.Visit(this);
        }
        public virtual LTSQLToken VisitToken(LTSQLToken token)
        {
            return _visitor(token);
        }
        public virtual LTSQLToken VisitSqlParameterToken(SqlParameterToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitObjectToken(ObjectToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitAliasToken(AliasToken token)
        {
            return this._visitor(token); 
        }
        public virtual LTSQLToken VisitBinaryToken(BinaryToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitBoolCalcToken(BoolCalcToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitConstantToken(ConstantToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitSyntaxToken(SyntaxToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitNullToken(NullToken token)
        {
            return this._visitor(token); 
        }
        public virtual LTSQLToken VisitJoinToken(JoinToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitFunctionCallToken(FunctionCallToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitObjectAccessToken(ObjectAccessToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitSelectToken(SelectToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitSqlQueryToken(SqlQueryToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitPageToken(PageToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitSQLScopeToken(SqlScopeToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitPriorityCalcToken(PriorityCalcToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitSequenceToken(SequenceToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitClauseToken(ClauseToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitValuesListToken(ValuesListToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitFieldListToken(FieldListToken token)
        {
            return this._visitor(token);
        }
    }
}
 