using MNet.LTSQL.v1.SqlTokens;
using MNet.Utils;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Xml.Linq;

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
        private LTSQLToken[] PopAsParamters(int cnt)
        {
            if (cnt <= 0)
                return new LTSQLToken[0];

            Stack<LTSQLToken> args = new Stack<LTSQLToken>();
            for (int i = 0; i < cnt; i++)
                args.Push(this.PopToken());

            return args.ToArray();
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
        private object PropOrFieldValue(MemberInfo member, object? inst)
        {
            if (member is PropertyInfo prop)
                return prop.GetValue(inst);
            else if (member is FieldInfo field)
                return field.GetValue(inst);
            else
                throw new Exception($"非字段或者属性无法求值：{member.Name}");
        }
        private bool IsObjectPrefix(string objPrefix)
        {
            LTSQLContext context = this._context;
            LTSQLScope scope = this._scope;
            var map = this._context.ObjectPrefix;
            while (!map.Contain(objPrefix))
            {
                if (scope.Parent == null || scope.Parent.Context == null)
                    throw new Exception($"无法找到对应的实体类映射:{objPrefix}");

                scope = scope.Parent;
                context = scope.Context;
                map = context.ObjectPrefix;
            }

            return map.IsObjectPrefix(objPrefix);
        }
        private string GetTableName(string objPrefix)
        {
            LTSQLContext context = this._context;
            LTSQLScope scope = this._scope;
            var map = this._context.ObjectPrefix;
            while (!map.Contain(objPrefix))
            {
                if (scope.Parent == null || scope.Parent.Context == null)
                    throw new Exception($"无法找到对应的实体类映射:{objPrefix}");

                scope = scope.Parent;
                context = scope.Context;
                map = context.ObjectPrefix;
            }

            return map.GetTableName(objPrefix);
        }

        

        //递归分配表名
        private void AssignTableAlias()
        {
            QuerySequence complex = this._context.Root;
            if (complex == null)
                return;

            string root = "p" + this._context.TableNameGenerator.Next();

            //分配表名
            Dictionary<string, string> param2table = new Dictionary<string, string>();
            HashSet<string> objPrefixs = new HashSet<string>();
            
            //涉及联表
            if (complex.From is FromJoinUnit join)
            {
                this.AssignFromJoinAlias(join, param2table, objPrefixs, root);
            }
            //单表
            else
            {
                param2table[root] = root;
                complex.From.Source.Alias = root;
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

                join.Source.Alias = param2table[p2];
                this.AssignFromJoinAlias(join.From, param2table, prefixs, p1);
            }
            else
            {
                param2table[prefix] = this._context.TableNameGenerator.Next();
                from.Source.Alias = param2table[prefix];
            }
        }
        // 调用外部翻译扩展
        private bool OnTranslateExpression(TranslateContext ctx)
        {
            this._context.LTSQLTranslater.TranslateExpression(ctx);
            if (ctx.ResultToken != null)
                this.PushToken(ctx.ResultToken);

            return ctx.ResultToken != null;
        }
        // 调用外部翻译扩展
        private bool OnTranslateMember(TranslateContext ctx)
        {
            this._context.LTSQLTranslater.TranslateMember(ctx);
            if (ctx.ResultToken != null)
                this.PushToken(ctx.ResultToken);

            return ctx.ResultToken != null;
        }


        
        //处理 from 子句
        private FromToken TranslateFrom(FromUnit from)
        {
            FromToken fromToken = null;
            FromToken leftToken = null;
            LTSQLToken query = null;

            if (from.Source is QuerySequence)
            {
                query = new SequenceTranslater()
                       .Translate((QuerySequence)from.Source, this._scope.NewScope());
            }
            else if (from.Source is SimpleSequence simple)
            {
                query = new AliasToken(simple.Type.Name) { ValueType = simple.Type };
            }
            else
            {
                throw new Exception($"不支持的查询结构:{from.Source.GetType().FullName}");
            }

            //联表
            if (from is FromJoinUnit join)
            {
                //联接条件

                //Expression joinOn = Expression.MakeBinary(ExpressionType.Equal, join.Source1Key, join.Source2Key);
                //this.Visit(joinOn);

                LTSQLToken where = this.PopToken();

                leftToken = this.TranslateFrom(join.From);
                fromToken = new FromJoinToken
                {
                    JoinType = "LEFT",
                    From = leftToken,
                    Source = new AliasTable(join.Source.Alias, query),
                    JoinOn = new WhereToken() { Condition = where },
                    //ValueType = join.JoinExpr.Type
                };
            }
            //单表
            else
            {
                fromToken = new FromToken();
                //fromToken.ValueType = from.Source.Type;
                fromToken.Source = new AliasTable(from.Source.Alias, query);
            }

            return fromToken;
        }


        //开始翻译
        private void TranslateCore()
        {
            //分配表名
            this.AssignTableAlias();

            SqlQueryToken queryToken = new SqlQueryToken();

        }


        //翻译参数
        protected override Expression VisitParameter(ParameterExpression node)
        {
            string objprefix = node.Name;
            if (this.IsObjectPrefix(objprefix))
            {
                //忽略掉join 过程中的 属性前缀链
                this.PushToken(new AliasToken()
                {
                    ValueType = node.Type,
                    Alias = objprefix
                });
            }
            else
            {
                string tableName = this.GetTableName(objprefix);
                TranslateContext ctx = this.NewTranslateContext();
                ctx.TranslateExpr = node;
                ctx.MemberOnwerType = node.Type;

                //外部转换优先
                if (!this.OnTranslateExpression(ctx))
                {
                    //默认转换
                    this.PushToken(new AliasToken(tableName)
                    {
                        ValueType = node.Type
                    });
                }
            }

            return base.VisitParameter(node);
        }
        //常量
        protected override Expression VisitConstant(ConstantExpression node)
        {
            var transCtx = this.NewTranslateContext();
            transCtx.TranslateExpr = node;
            transCtx.MemberOnwerType = node.Type;

            if (this.OnTranslateExpression(transCtx))
            {
                return node;
            }

            if (node.Value == null)
            {
                this.PushToken(new ConstantToken("null", node.Type));
            }
            else
            {
                this.PushToken(new SqlParameterToken(this._context.ParameterNameGenerator.Next(), node.Value) { ValueType = node.Type });
            }

            return base.VisitConstant(node);
        }
        //字段或者属性
        protected override Expression VisitMember(MemberExpression node)
        {
            Expression expr = base.VisitMember(node);

            //静态成员
            if (node.Expression == null)
            {
                //外部对表达式树翻译
                var transCtx1 = this.NewTranslateContext();
                transCtx1.TranslateExpr = node;
                transCtx1.MemberOnwerType = node.Expression?.Type ?? node.Member.ReflectedType;
                if (this.OnTranslateExpression(transCtx1))
                    return expr;

                //外部对成员调用翻译
                var transCtx2 = this.NewTranslateContext();
                transCtx2.TranslateExpr = node;
                transCtx2.TranslateMember = node.Member;
                transCtx2.MemberOnwerType = node.Member.ReflectedType;
                if (this.OnTranslateMember(transCtx2))
                    return expr;

                object val = this.PropOrFieldValue(node.Member, null);
                this.PushToken(new SqlParameterToken(this._context.ParameterNameGenerator.Next(), val) { ValueType = node.Type });
                return expr;
            }


            /*非静态成员*/
            string memberName = node.Member.Name;
            LTSQLToken token = this.PopToken();
            if (token == null)
                throw new Exception($"表达式解析结果为null:{node}");


            //表名转换
            if (token is AliasToken alias && this.IsObjectPrefix(alias.Alias))
            {
                string accessPath = $"{alias.Alias}.{memberName}";
                //忽略掉 join 过程中的 属性前缀链
                if (this.IsObjectPrefix(accessPath))
                {
                    this.PushToken(new AliasToken(accessPath) { ValueType = node.Type });
                }
                // join 过程中的 属性前缀链 转化成表名
                else
                {
                    //外部对表达式树翻译
                    var transCtx1 = this.NewTranslateContext();
                    transCtx1.TranslateExpr = node;
                    transCtx1.MemberOnwerType = node.Expression?.Type ?? node.Member.ReflectedType;
                    if (this.OnTranslateExpression(transCtx1))
                        return expr;

                    string tableName = this.GetTableName(accessPath);
                    this.PushToken(new AliasToken(tableName) { ValueType = node.Type });
                }
            }
            //字段访问
            else
            {
                //外部对表达式树翻译
                var transCtx1 = this.NewTranslateContext();
                transCtx1.TranslateExpr = node;
                transCtx1.MemberOnwerType = node.Expression?.Type ?? node.Member.ReflectedType;
                if (this.OnTranslateExpression(transCtx1))
                    return expr;

                //对常量求值
                if (token is SqlParameterToken p)
                {
                    object obj = p.Value;
                    if (obj == null)
                        throw new Exception($"表达式不能依赖null对象求值：{obj}");


                    var transCtx2 = this.NewTranslateContext();
                    transCtx2.TranslateExpr = node;
                    transCtx2.MemberOwner = obj;
                    transCtx2.MemberOnwerType = node.Expression.Type;
                    transCtx2.TranslateMember = node.Member;
                    if (!this.OnTranslateMember(transCtx2))
                    {
                        //对象访问
                        object val = this.PropOrFieldValue(node.Member, obj);
                        this.PushToken(new SqlParameterToken(p.ParameterName, val) { ValueType = node.Type });
                    }
                }
                else
                {
                    var transCtx2 = this.NewTranslateContext();
                    transCtx2.TranslateExpr = node;
                    transCtx2.MemberOnwerType = node.Expression.Type;
                    transCtx2.TranslateMember = node.Member;
                    if (!this.OnTranslateMember(transCtx2))
                    {
                        //对象访问
                        this.PushToken(new ObjectAccessToken(token, new AliasToken(memberName)) { ValueType = node.Type });
                    }
                }
            }
            
            return expr;
        }
        //函数调用
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Expression expr = base.VisitMethodCall(node);
            //外部表达式树翻译
            var transCtx1 = this.NewTranslateContext();
            transCtx1.TranslateExpr = node;
            transCtx1.MemberOnwerType = node.Method.ReflectedType;
            if (this.OnTranslateExpression(transCtx1))
                return expr;


            object val = null;
            LTSQLToken token = null;
            LTSQLToken objToken = null;
            LTSQLToken[] parameters = null;

            //静态方法的调用
            if (node.Object == null)
            {
                //外部成员翻译
                var transCtx2 = this.NewTranslateContext();
                transCtx2.TranslateExpr = node;
                transCtx2.TranslateMember = node.Method;
                transCtx2.MemberOnwerType = node.Method.ReflectedType;
                if (this.OnTranslateMember(transCtx2))
                    return expr;

                //参数个数为0的静态方法，直接调用求值
                if (node.Arguments.Count == 0)
                {
                    val = node.Method.Invoke(null, null);
                    token = new SqlParameterToken(this._context.ParameterNameGenerator.Next(), val) { ValueType = node.Method.ReturnType };
                    this.PushToken(token);
                    return expr;
                }

                parameters = this.PopAsParamters(node.Arguments.Count);
                if (!parameters.All(p => p is SqlParameterToken))
                    throw new Exception($"静态方法引用动态参数无法值：{node}");

                val = node.Method.Invoke(null, parameters.Select(p => ((SqlParameterToken)p).Value).ToArray());
                token = new SqlParameterToken(this._context.ParameterNameGenerator.Next(), val) { ValueType = node.Method.ReturnType };
                return expr;
            }


            /* 实力方法调用*/
            MethodInfo method = node.Method;
            objToken = this.PopToken();

            var transCtx3 = this.NewTranslateContext();
            transCtx3.TranslateExpr = node;
            transCtx3.TranslateMember = node.Method;
            transCtx3.MemberOwner = objToken is SqlParameterToken ? ((SqlParameterToken)objToken).Value : null;
            transCtx3.MemberOnwerType = node.Object.Type;
            if (this.OnTranslateMember(transCtx3))
                return expr;

            //实例对象求值
            parameters = this.PopAsParamters(node.Arguments.Count);
            if (objToken is SqlParameterToken inst)
            {
                if (parameters.IsNotEmpty() && !parameters.All(p => p is SqlParameterToken))
                    throw new Exception($"实例方法无法求值：{node}");
                if (inst.Value == null)
                    throw new Exception($"实例对象为null，无法求值：{node}");

                val = node.Method.Invoke(inst.Value, parameters.Select(p => ((SqlParameterToken)p).Value).ToArray());
                token = new SqlParameterToken(this._context.ParameterNameGenerator.Next(), val) { ValueType = node.Method.ReturnType };
                this.PushToken(token);
                return expr;
            }

            //sql 函数调用
            token = new FunctionToken(node.Method.Name)
            {
                ValueType = node.Method.ReturnType,
                Parameters = parameters.ToList()
            };
            this.PushToken(token);

            return expr;
        }
        //new 表达式
        protected override Expression VisitNew(NewExpression node)
        {
            return base.VisitNew(node);
        }
        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            return base.VisitMemberBinding(node);
        }
        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            return base.VisitMemberMemberBinding(node);
        }


        //二元表达式
        protected override Expression VisitBinary(BinaryExpression node)
        {
            Expression expr = base.VisitBinary(node);

            var transCtx = this.NewTranslateContext();
            transCtx.TranslateExpr = node;
            transCtx.MemberOnwerType = node.Type;

            if (this.OnTranslateExpression(transCtx))
                return expr;

            switch (node.NodeType)
            {
                case ExpressionType.Add:
                    break;
                case ExpressionType.Equal:
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    break;
                case ExpressionType.LessThanOrEqual:
                    break;
                case ExpressionType.LessThan:
                    break;
                case ExpressionType.GreaterThan:
                    break;
                case ExpressionType.AndAlso:
                    break;
                case ExpressionType.OrElse:
                    break;
                default:
                    throw new NotImplementedException($"暂不支持此二元表达式翻译：{node.NodeType}");
            }

            return expr;
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
