using MNet.LTSQL.v1.SqlTokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL.v1
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
                var _new = t.VisitChildren(v);
                return visitor(t) ?? _new;
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
        public virtual LTSQLToken VisitAliasTableToken(AliasTableToken token)
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
        public virtual LTSQLToken VisitConditionToken(BoolCalcToken token)
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
        public virtual LTSQLToken VisitFromToken(FromToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitJoinToken(JoinToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitFunctionToken(FunctionToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitGroupToken(GroupToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitObjectAccessToken(ObjectAccessToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitOrderByItemToken(OrderByItemToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitOrderToken(OrderToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitSelectToken(SelectToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitSelectItemToken(SelectItemToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitSqlQueryToken(SqlQueryToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitWhereToken(WhereToken token)
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
        public virtual LTSQLToken VisitTokenItemListToken(TokenItemListToken token)
        {
            return this._visitor(token);
        }
        public virtual LTSQLToken VisitSequenceToken(SequenceToken token)
        {
            return this._visitor(token);
        }
    }
}
 