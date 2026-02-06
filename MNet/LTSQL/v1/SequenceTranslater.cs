using MNet.LTSQL.v1.SqlTokens;
using MNet.Utils;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

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
        //生成的SQL令牌栈
        private Stack<LTSQLToken> _tokens;
        private TranslateContext _templateContext;


        private void PushToken(LTSQLToken token)
        {
            this._tokens.Push(token);
        }
        private LTSQLToken PopToken()
        {
            return this._tokens.Count > 0 ? this._tokens.Pop() : null;
        }
        private LTSQLToken PeekToken()
        {
            return this._tokens.Peek();
        }
        private TranslateContext NewTranslateContext()
        {
            this._templateContext ??= new TranslateContext(this._tokens);
            this._templateContext.ClearProps();

            this._templateContext.Tokens = this._tokens;
            this._templateContext.Options = this._context.Options;
            this._templateContext.ParameterNameGenerator = this._context.ParameterNameGenerator;

            return this._templateContext;
        }


        //递归分配表名
        private void AssignTableAlias()
        {
            QuerySequence complex = this._context.Root as QuerySequence;
            if (complex == null)
                return;

            string root = "p" + this._context.TableNameGenerator.Next();

            //分配表名
            Dictionary<string, string> param2table = new Dictionary<string, string>();
            HashSet<string> objPrefixs = new HashSet<string>();
            if (complex.From is FromJoinUnit join)
            {
                this.AssignFromJoinAlias(join, param2table, objPrefixs, root);
            }
            else
            {
                param2table[root] = root;
            }


            //统一参数名
            ExpressionModifier exprModifier = new ExpressionModifier();
            ParameterExpression newParameter = Expression.Parameter(complex.Type, root);

            if (complex.Where != null && complex.Where.Conditions.IsNotEmpty())
            {
                LambdaExpression merge = null;
                foreach (Expression expr in complex.Where.Conditions)
                {
                    LambdaExpression lambda = expr as LambdaExpression;
                    ParameterExpression paramter = lambda.Parameters[0];

                    LambdaExpression newExpr = exprModifier.VisitParameter(expr, p => object.ReferenceEquals(paramter, p) ? newParameter : p) as LambdaExpression;

                    //多条件合并
                    merge = merge == null ? newExpr : Expression.Lambda(Expression.And(merge.Body, newExpr.Body), newParameter);
                }
                complex.Where.Conditions.Clear();
                complex.Where.Conditions.Add(merge);
            }

            //分组
            if(complex?.Group?.GroupKeys != null)
            {
                ParameterExpression _old = (complex.Group.GroupKeys as LambdaExpression).Parameters[0];
                complex.Group.GroupKeys = exprModifier.VisitParameter(complex.Group.GroupKeys, p => object.ReferenceEquals(_old, p) ? newParameter : p);
            }

            //排序
            if (complex?.Order?.OrderKeys?.IsNotEmpty() ?? false)
            {
                List<KeyValuePair<Expression, bool>> list = new List<KeyValuePair<Expression, bool>>();
                foreach (var orderItem in complex.Order.OrderKeys)
                {
                    ParameterExpression _old = (orderItem.Key as LambdaExpression).Parameters[0];
                    Expression newExpr = exprModifier.VisitParameter(orderItem.Key, p => object.ReferenceEquals(p, _old) ? newParameter : p);
                    list.Add(new KeyValuePair<Expression, bool>(newExpr, true));
                }
            }

            //投影
            if (complex.Select?.SelectKey != null)
            {
                ParameterExpression _old = (complex.Select.SelectKey as LambdaExpression).Parameters[0];
                complex.Select.SelectKey = exprModifier.VisitParameter(complex.Select.SelectKey, p => object.ReferenceEquals(_old, p) ? newParameter : p);
            }

            //
            this._context.ObjectPrefix = new LTSQLTableNameMapping(param2table, objPrefixs);
        }
        private void AssignFromJoinAlias(FromUnit from, Dictionary<string, string> param2table, HashSet<string> prefixs, string prefix)
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

                prefixs.Add(p1);
                prefixs.Add(p2);
                param2table[p2] = this._context.TableNameGenerator.Next();
                this.AssignFromJoinAlias(join.From, param2table, prefixs, p1);
            }
            else
            {
                param2table[prefix] = this._context.TableNameGenerator.Next();
            }
        }
        private void TranslateCore()
        {
            //分配表名
            this.AssignTableAlias();

        }

        private bool TranslateExpression(TranslateContext ctx)
        {
            this._context.LTSQLTranslater.TranslateExpression(ctx);
            if (ctx.ResultToken != null)
                this.PushToken(ctx.ResultToken);

            return ctx.ResultToken != null;
        }


        protected override Expression VisitParameter(ParameterExpression node)
        {
            string objprefix = node.Name;
            if (this._context.ObjectPrefix.IsObjectPrefix(objprefix))
            {
                this.PushToken(new AliasToken()
                {
                    Type = node.Type,
                    Alias = objprefix
                });
            }
            else
            {
                string tableName = this._context.ObjectPrefix.GetTableName(objprefix);
                TranslateContext ctx = this.NewTranslateContext();
                ctx.TranslateExpr = node;
                ctx.MemberOnwerType = node.Type;

                //外部转换优先
                if (!this.TranslateExpression(ctx))
                {
                    //默认转换
                    this.PushToken(new AliasToken(tableName)
                    {
                        Type = node.Type
                    });
                }
            }

            return base.VisitParameter(node);
        }
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node == null)
                this.PushToken(new ConstantToken("null"));
            else
                this.PushToken(new SqlParameterToken(this._context.ParameterNameGenerator.Next(), node.Value));

            return base.VisitConstant(node);
        }
        protected override Expression VisitMember(MemberExpression node)
        {
            if(node.Expression == null)
            {
                //静态成员访问
                // TODO
            }

            Expression expr = base.VisitMember(node);

            //属性 或者 字段
            string memberName = node.Member.Name;
            LTSQLToken token = this.PopToken();
            if (token == null)
                throw new Exception($"表达式解析结果为null:{node}");

            if (token is SqlParameterToken p)
            {
                //对常量求值
                object obj = p.Value;
                if (obj == null)
                    throw new Exception($"表达式不能依赖null对象求值：{obj}");

                object val = null;
                if (node.Member is FieldInfo field)
                {
                    val = field.GetValue(obj);
                }
                else if (node.Member is PropertyInfo prop)
                {
                    val = prop.GetValue(obj);
                }
                else
                {
                    throw new Exception($"表达式类型不是属性或者字段访问，求值：{node}");
                }

                p.Value = val;
                this.PushToken(p);
            }
            else if (token is AliasToken obj)
            {
                //sql 对象的字段访问
                this.PushToken(new ObjectAccessToken()
                {
                    Owner = obj,
                    Field = new AliasToken(memberName)
                });
            }
            else if (token is ObjectAccessToken access)
            {
                // join 对象的属性前缀剔除
                string objprefix = $"{access.Owner.Alias}.{access.Field.Alias}";
                string tableName = this._context.ObjectPrefix.GetTableName($"{objprefix}.{memberName}");
                if (tableName == null)
                {
                    this.PushToken(new ObjectAccessToken()
                    {
                        Owner = new AliasToken(objprefix),
                        Field = new AliasToken(tableName)
                        {
                            //Type = 
                        }
                    });
                }
                else
                {
                    //转化成表
                    this.PushToken(new AliasToken(tableName));
                }
            }
            else
            {
                throw new Exception($"表达式解析出无效的token类型：{node.Expression}");
            }
            return expr;
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if(node.Object == null)
            {
                //静态方法的调用
            }

            MethodInfo method = node.Method;
            LTSQLToken objToken = null;
            List<LTSQLToken> args = new List<LTSQLToken>();

            if (node.Object != null)
            {
                this.Visit(node.Object);
                objToken = this.PopToken();
                if (objToken == null)
                    throw new Exception($"无法null对象调用实例方法：{node}");
            }

            foreach (Expression arg in node.Arguments)
            {
                this.Visit(arg);
                LTSQLToken argToken = this.PopToken();
                args.Add(argToken);
            }

            //this._context.Options.SQLTokenTranslaters.Select(null, method);

            return node;
        }


        public LTSQLToken Translate(QuerySequence query, LTSQLOptions options)
        {
            return this.Translate(query, new LTSQLScope()
            {
                Context = new LTSQLContext()
                {
                    Options = options,
                    TableNameGenerator = new NameGenerator(i => $"t{i}"),
                    ParameterNameGenerator = new NameGenerator(i => $"p{i}"),
                    LTSQLTranslater = new CombineTranslaterSelector(options?.SQLTokenTranslaters, new LTSQLTokenTranslaterSelector())
                }
            });
        }
        internal LTSQLToken Translate(QuerySequence query, LTSQLScope scope)
        {
            query = query.UnWrap();

            this._scope = scope;
            this._context = scope.Context;
            this._context.Root = query;

            this._flags = new Stack<bool>();
            this._tokens = new Stack<LTSQLToken>();

            this.TranslateCore();
            return null;
        }
    }
}
