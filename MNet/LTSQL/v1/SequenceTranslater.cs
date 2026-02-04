using MNet.LTSQL.v1.SqlTokens;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;

namespace MNet.LTSQL.v1
{
    //开启翻译
    public class SequenceTranslater : ExpressionVisitor
    {
        public SequenceTranslater()
        { }


        private LTSQLScope _scope;
        private LTSQLContext _context;

        //是否需要常量求值
        private Stack<bool> _flags;
        //常量对象栈
        private Stack<object> _objs;
        //生成的SQL令牌栈
        private Stack<LTSQLToken> _tokens;



        //递归分配表名
        private void AssignTableAlias()
        {
            QuerySequence complex = this._context.Root as QuerySequence;
            if (complex == null)
                return;

            string root = "p" + this._context.TableNameGenerator.Next();

            //分配表名
            Dictionary<string, string> param2table = new Dictionary<string, string>();
            if (complex.From is FromJoinUnit join)
                this.AssignFromJoinAlias(join, param2table, root);
            else
                param2table[root] = root;
            
             

        }
        private void AssignFromJoinAlias(FromUnit from, Dictionary<string, string> param2table, string prefix)
        {
            if (from is FromJoinUnit join)
            {
                string p1 = (join.Source1Key as LambdaExpression).Parameters[0].Name;
                string p2 = (join.Source2Key as LambdaExpression).Parameters[0].Name;
                if (!string.IsNullOrEmpty(prefix))
                {
                    p1 = $"{prefix}.{p1}";
                    p2 = $"{prefix}.{p2}";
                }

                param2table[p2] = this._context.TableNameGenerator.Next();
                this.AssignFromJoinAlias(join.From, param2table, string.IsNullOrWhiteSpace(prefix) ? p1 : $"{prefix}.{p1}");
            }
            else
            {
                param2table[prefix] = this._context.TableNameGenerator.Next();
            }
        }



        public LTSQLToken Translate(Sequence query)
        {
            return this.Translate(query, new LTSQLScope()
            {
                Context = new LTSQLContext()
                {
                    TableNameGenerator = new NameGenerator(i => $"t{i}"),
                    ParameterNameGenerator = new NameGenerator(i => $"p{i}")
                }
            });
        }
        public LTSQLToken Translate(Sequence query, LTSQLScope scope)
        {
            if (query is QuerySequence complex)
                query = complex.UnWrap();

            this._scope = scope;
            this._context = scope.Context;
            this._context.Root = query;

            this._flags = new Stack<bool>();
            this._objs = new Stack<object>();
            this._tokens = new Stack<LTSQLToken>();

            //分配表名
            this.AssignTableAlias();

            return null;
        }
    }
}
