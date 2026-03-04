using MNet.LTSQL.v1.SqlTokens;
using MNet.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
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
        //生成的SQL令牌栈
        private Stack<LTSQLToken> _tokens;
        //复用对象，用于扩展翻译上下文，避免频繁创建对象
        private TranslateContext _templateContext;
        private List<(Expression, LTSQLToken)> _tokenInsteadList;


        private LTSQLToken PopToken()
        {
            return this._tokens.Count > 0 ? this._tokens.Pop() : null;
        }
        private LTSQLToken PeekToken()
        {
            return this._tokens.Peek();
        }
        private void PushToken(LTSQLToken token)
        {
            this._tokens.Push(token);
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
        private TranslateContext NewTranslateContext()
        {
            this._templateContext ??= new TranslateContext(this._tokens);
            this._templateContext.ClearProps();

            this._templateContext.Tokens = this._tokens;
            this._templateContext.Options = this._context.Options;
            this._templateContext.ParameterNameGenerator = this._context.ParameterNameGenerator;

            return this._templateContext;
        }
        private TranslateContext NewTranslateContext(Expression expr, Type exprType)
        {
            TranslateContext ctx = this.NewTranslateContext();
            ctx.TranslateExpr = expr;
            ctx.ExpressionValueType = exprType;

            return ctx;
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
        private TableAliasMapping GetRootTableAliasMapping(string parameterName)
        {
            LTSQLContext context = this._context;
            LTSQLScope scope = this._scope;
            while (context.TableAliasMapping.PropName != parameterName)
            {
                if (scope.Parent == null || scope.Parent.Context == null)
                    throw new Exception($"参数名({parameterName})无法找到对应的上下文作用域, 无法解析表命名");

                scope = scope.Parent;
                context = scope.Context;
            }

            return context.TableAliasMapping;
        }
        private void UnUseSpecialToken(Expression expr)
        {
            if (this._tokenInsteadList == null)
                return;
            this._tokenInsteadList.RemoveAll(p => object.ReferenceEquals(p.Item1, expr));
        }
        private LTSQLToken GetSpecialTokenInstead(Expression expr)
        {
            if (this._tokenInsteadList == null)
                return null;

            return this._tokenInsteadList.FirstOrDefault(p => object.ReferenceEquals(p.Item1, expr)).Item2;
        }
        private void UseSpecialTokenInstead(Expression expr, LTSQLToken token)
        {
            this._tokenInsteadList ??= new List<(Expression, LTSQLToken)>();
            this._tokenInsteadList.Add((expr, token));
        }

        


        //递归分配表名
        private void AssignTableAlias()
        {
            QuerySequence complex = this._context.Root;
            if (complex == null)
                return;

            bool bGroupFlag = false;
            string root = "p" + this._context.TableNameGenerator.Next();

            //分配表名
            Dictionary<string, string> param2table = new Dictionary<string, string>();
            HashSet<string> objPrefixs = new HashSet<string>() { root };

            TableAliasMapping mapping = null;

            //涉及联表
            if (complex.From is FromJoinUnit join)
            {
                mapping = new TableAliasMapping(root);

                ParameterExpression joinObj = Expression.Parameter(((LambdaExpression)join.JoinExpr).Body.Type, root);
                this.AssignFromJoinAlias(mapping, join, joinObj, joinObj);
            }
            //单表
            else
            {
                complex.From.Source.Alias = root;
                mapping = new TableAliasMapping(root, root);
            }


            //统一根参数名
            ExpressionModifier exprModifier = new ExpressionModifier();
            Type unifyParameterType = complex.Type;
            ParameterExpression newParameter = Expression.Parameter(unifyParameterType, root);

            if (complex.Where != null && complex.Where.Conditions.IsNotEmpty())
            {
                //where 多条件合并
                LambdaExpression merge = null;
                foreach (Expression expr in complex.Where.Conditions)
                {
                    LambdaExpression lambda = expr as LambdaExpression;
                    ParameterExpression paramter = lambda.Parameters[0];
                    LambdaExpression newExpr = exprModifier.VisitParameter(expr, p => object.ReferenceEquals(paramter, p) ? newParameter : p) as LambdaExpression;

                    merge = merge == null ? newExpr : Expression.Lambda(Expression.And(merge.Body, newExpr.Body), newParameter);
                }
                complex.Where.Conditions.Clear();
                complex.Where.Conditions.Add(merge);
            }

            //分组
            if (complex?.Group?.GroupKeys != null)
            {
                bGroupFlag = true;
                ParameterExpression _old = (complex.Group.GroupKeys as LambdaExpression).Parameters[0];
                complex.Group.GroupKeys = exprModifier.VisitParameter(complex.Group.GroupKeys, p => object.ReferenceEquals(_old, p) ? newParameter : p);
                if (complex.Group.ElementExpr != null)
                {
                    _old = (complex.Group.ElementExpr as LambdaExpression).Parameters[0];
                    complex.Group.ElementExpr = exprModifier.VisitParameter(complex.Group.ElementExpr, p => object.ReferenceEquals(_old, p) ? newParameter : p);
                }
            }

            // having


            //排序
            if (complex?.Order?.OrderKeys?.IsNotEmpty() ?? false)
            {
                List<KeyValuePair<Expression, bool>> list = new List<KeyValuePair<Expression, bool>>();
                foreach (var orderItem in complex.Order.OrderKeys)
                {
                    ParameterExpression _old = (orderItem.Key as LambdaExpression).Parameters[0];
                    Expression newExpr = exprModifier.VisitParameter(orderItem.Key, p => object.ReferenceEquals(p, _old) ? newParameter : p);
                    list.Add(new KeyValuePair<Expression, bool>(newExpr, orderItem.Value));
                }

                complex.Order.OrderKeys = list;
            }

            //投影（仅在不存在分组的情况下才有替换参数的意义）
            if (complex.Select?.SelectKey != null && !bGroupFlag)
            {
                ParameterExpression _old = (complex.Select.SelectKey as LambdaExpression).Parameters[0];
                complex.Select.SelectKey = exprModifier.VisitParameter(complex.Select.SelectKey, p => object.ReferenceEquals(_old, p) ? newParameter : p);
            }

            //
            this._context.GroupFlag = bGroupFlag;
            this._context.TableAliasMapping = mapping;
        }
        private void AssignFromJoinAlias(TableAliasMapping mapping, FromUnit from, Expression obj, ParameterExpression root)
        {
            if (from is FromJoinUnit join)
            {
                //构造 join
                LambdaExpression getJoinKey1 = join.Source1Key as LambdaExpression;
                LambdaExpression getJoinKey2 = join.Source2Key as LambdaExpression;

                Expression accessJoinKey1 = Expression.MakeMemberAccess(obj, obj.Type.GetMember(getJoinKey1.Parameters[0].Name)[0]);
                Expression accessJoinKey2 = Expression.MakeMemberAccess(obj, obj.Type.GetMember(getJoinKey2.Parameters[0].Name)[0]);

                ExpressionModifier modifier = new ExpressionModifier();
                Expression joinKey1 = modifier.VisitParameter(getJoinKey1.Body, p => object.ReferenceEquals(p, getJoinKey1.Parameters[0]) ? accessJoinKey1 : p);
                Expression joinKey2 = modifier.VisitParameter(getJoinKey2.Body, p => object.ReferenceEquals(p, getJoinKey2.Parameters[0]) ? accessJoinKey2 : p);
                Expression joinEqual = Expression.Lambda(Expression.Equal(joinKey1, joinKey2), root);


                string p1 = getJoinKey1.Parameters[0].Name;
                string p2 = getJoinKey2.Parameters[0].Name;
                TableAliasMapping mapping1 = new TableAliasMapping(p1);
                
                //next
                this.AssignFromJoinAlias(mapping1, join.From, accessJoinKey1, root);
                
                string alias = this._context.TableNameGenerator.Next();
                TableAliasMapping mapping2 = new TableAliasMapping(alias, p2);

                mapping.Props.Add(mapping1);
                mapping.Props.Add(mapping2);

                join.JoinOn = joinEqual;
                join.Source.Alias = alias;
            }
            else
            {
                //
                string alias = this._context.TableNameGenerator.Next();

                mapping.Alias = alias;
                from.Source.Alias = alias;
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
        private bool OnTranslateExpression(Expression expr, Type exprType = null)
        {
            TranslateContext ctx = this.NewTranslateContext(expr, exprType ?? expr.Type);
            return this.OnTranslateExpression(ctx);
        }
        // 调用外部翻译扩展
        private bool OnTranslateMember(TranslateContext ctx)
        {
            this._context.LTSQLTranslater.TranslateMember(ctx);
            if (ctx.ResultToken != null)
                this.PushToken(ctx.ResultToken);
            
            return ctx.ResultToken != null;
        }
        private bool OnTranslateMember(MemberInfo member, object owner, Type ownerType, Expression expr, Type exprType = null, LTSQLToken ownerToken = null, LTSQLToken[] memberCallParameters = null)
        {
            TranslateContext ctx = this.NewTranslateContext(expr, exprType ?? expr.Type);
            ctx.Member = member;
            ctx.Owner = owner;
            ctx.OwnerType = ownerType;
            ctx.OwnerToken = ownerToken;
            ctx.MethodParameterTokenList = memberCallParameters;

            return this.OnTranslateMember(ctx);
        }



        //处理 from 子句
        private FromToken TranslateFrom(FromUnit from, ref List<SelectItemToken> fields)
        {
            FromToken fromToken = null;
            FromToken leftToken = null;
            LTSQLToken query = null;

            //联表
            if (from is FromJoinUnit join)
            {
                leftToken = this.TranslateFrom(join.From, ref fields);

                //联接条件   TODO 解决参数与表名对应问题           
                this.Visit((join.JoinOn as LambdaExpression).Body);
                LTSQLToken where = this.PopToken();
                fromToken = new FromJoinToken
                {
                    JoinType = "LEFT JOIN", //目前固定死，就是左连接
                    From = leftToken,
                    JoinOn = where,
                    SourceType = join.Source.Type
                };
            }
            //单表
            else
            {
                fromToken = new FromToken();
                fromToken.SourceType = from.Source.Type;
            }


            //嵌套子查询
            if (from.Source is QuerySequence)
            {
                query = new SequenceTranslater()
                       .Translate((QuerySequence)from.Source, this._scope.NewScope());

                //解析字段
                if (query is SqlQueryToken sqlquery)
                {
                    foreach (var selecdtFields in sqlquery.DefaultFields)
                    {
                        var fieldAccess = new ObjectAccessToken(new AliasToken(from.Source.Alias), selecdtFields.FieldAlias);
                        fields.Add(new SelectItemToken(fieldAccess, selecdtFields.FieldAlias));
                    }
                }
                query = new SqlScopeToken(query);
            }
            //单表访问
            else if (from.Source is SimpleSequence simple)
            {
                query = new AliasToken(simple.Type.Name) { ValueType = simple.Type };

                //解析字段
                foreach (PropertyInfo prop in simple.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var fieldAccess = new ObjectAccessToken(new AliasToken(simple.Alias), prop.Name);
                    fields.Add(new SelectItemToken(fieldAccess, prop.Name));
                }
                foreach (FieldInfo prop in simple.Type.GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    var fieldAccess = new ObjectAccessToken(new AliasToken(simple.Alias), prop.Name);
                    fields.Add(new SelectItemToken(fieldAccess, prop.Name));
                }
            }
            else
            {
                throw new Exception($"不支持的查询结构:{from.Source.GetType().FullName}");
            }


            fromToken.Source = new AliasTable(from.Source.Alias, query);
            return fromToken;
        }
        private WhereToken TranslateWhere(WhereUnit where)
        {
            if (where.Conditions.IsEmpty())
                return null;

            this.Visit((where.Conditions[0] as LambdaExpression).Body);

            LTSQLToken token = this.PopToken();
            return new WhereToken(token);
        }
        private GroupToken TranslateGroup(GroupUnit group, out LTSQLToken groupKey, out LTSQLToken groupEle)
        {
            groupKey = null;
            groupEle = null;

            //分组依据翻译
            if (group.GroupKeys != null)
            {
                this.Visit((group.GroupKeys as LambdaExpression).Body);
                groupKey = this.PopToken();
            }
            
            //分组元素翻译
            if (group.ElementExpr != null)
            {
                this.Visit((group.ElementExpr as LambdaExpression).Body);
                groupEle = this.PopToken();
            }
            
            GroupToken groupToken = new GroupToken();
            if (groupKey is TupleToken tuple)
                groupToken.GroupByItems = tuple.Props.ToArray();
            else
                groupToken.GroupByItems = new[] { groupKey };

            return groupToken;
        }
        private OrderToken TranslateOrder(OrderUnit order)
        {
            if (order.OrderKeys.IsEmpty())
                return null;

            var items = new List<OrderByItemToken>(); 
            foreach (var orderKey in order.OrderKeys)
            {
                this.Visit((orderKey.Key as LambdaExpression).Body);
                LTSQLToken token = this.PopToken();
                items.Add(new OrderByItemToken() { IsAsc = !orderKey.Value, Item = token });
            }

            OrderToken orderToken = new OrderToken();
            orderToken.OrderBy = new TokenItemListToken(items);
            return orderToken;
        }
        private SelectToken TranslateSelect(SelectUnit select)
        {
            bool bflag = false;
            LambdaExpression selectExpr = select.SelectKey as LambdaExpression;
            ParameterExpression parameter = selectExpr.Parameters[0];

            if (this._context.GroupFlag)
            {
                //构造分组对象，将分组键和分组元素作为属性，供后续的表达式访问，便于判断聚合函数处理的时机
                GroupObjToken groupToken = new GroupObjToken();
                groupToken.GroupKey = this._context.GroupKey;
                groupToken.Element = this._context.GroupElement;
                groupToken.ValueType = parameter.Type;
                this.UseSpecialTokenInstead(parameter, groupToken);
                bflag = true;
            }

            try
            {
                this.Visit(selectExpr.Body);

                LTSQLToken token = this.PopToken();
                SelectToken selectToken = new SelectToken();
                List<SelectItemToken> fields = new List<SelectItemToken>();
                if (token is TupleToken tuple)
                {
                    fields.AddRange(tuple.Items.Select(p => new SelectItemToken(p.Item1, p.Item2)));
                }
                else
                {
                    fields.Add(new SelectItemToken(token, null));
                }

                selectToken.Field = new TokenItemListToken(fields);
                return selectToken;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (bflag)
                    this.UnUseSpecialToken(parameter);
            }
        }
        //开始翻译
        private SqlQueryToken TranslateCore()
        {
            //分配表名
            this.AssignTableAlias();

            QuerySequence query = this._context.Root;

            SqlQueryToken queryToken = new SqlQueryToken();
            queryToken.ValueType = typeof(ILTSQLObjectQueryable<>).MakeGenericType(query.Type);
            queryToken.DefaultFields = new List<SelectItemToken>();


            List<SelectItemToken> fields = queryToken.DefaultFields;
            //from
            queryToken.From = this.TranslateFrom(query.From, ref fields);
            //where
            if (query.Where != null && query.Where.Conditions.IsNotEmpty())
                queryToken.Where = TranslateWhere(query.Where);
            //group by
            if (query.Group?.GroupKeys != null)
            {
                LTSQLToken groupKey = null;
                LTSQLToken groupEle = null;
                queryToken.Group = this.TranslateGroup(query.Group, out groupKey, out groupEle);

                this._context.GroupKey = groupKey;
                this._context.GroupElement = groupEle;
            }

            //order by
            if (query.Order?.OrderKeys.IsNotEmpty() ?? false)
            {
                queryToken.Order = this.TranslateOrder(query.Order);
            }

            //select
            if (query.Select?.SelectKey != null)
            {
                queryToken.Select = this.TranslateSelect(query.Select);
            }
            else
            {
                queryToken.Select = new SelectToken
                {
                    Field = new TokenItemListToken(fields)
                };
            }


            //内联查询翻译
            LTSQLTokenVisitor.Visit(queryToken, (t, nxt) =>
            {
                t = nxt(t);

                //如果存在内联查询，需要进一步翻译
                if (t is SqlParameterToken p && p.Value is ILTSQLObjectQueryable subquery)
                {
                    LTSQLToken subQueryToken = new SequenceTranslater()
                       .Translate(subquery.Query, this._scope.NewScope());
                    return new SqlScopeToken(subQueryToken);
                }
                return t;
            });

            return queryToken;
        }



        //翻译参数
        protected override Expression VisitParameter(ParameterExpression node)
        {
            //内部token替换
            LTSQLToken tokenInstead = this.GetSpecialTokenInstead(node);
            if (tokenInstead != null)
            {
                this.PushToken(tokenInstead);
                return node;
            }

            //确定参数范围
            TableAliasMapping mapping = this.GetRootTableAliasMapping(node.Name);
            if(mapping.Alias == null)
            {
                //忽略掉join 过程中的 属性前缀链
                this.PushToken(new PrefixPropToken(node.Name)
                {
                    ValueType = node.Type,
                    AliasMapping = mapping
                });
            }
            else
            {
                //外部转换优先
                if (!this.OnTranslateExpression(node, node.Type))
                {
                    //默认转换
                    string tableName = mapping.Alias;
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
            if (this.OnTranslateExpression(node, node.Type))
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
                if (this.OnTranslateExpression(node, node.Type))
                    return expr;

                //外部对成员调用翻译
                if (this.OnTranslateMember(node.Member, null, node.Member.ReflectedType, node, node.Type, null, null))
                    return expr;

                object val = this.PropOrFieldValue(node.Member, null);
                this.PushToken(new SqlParameterToken(this._context.ParameterNameGenerator.Next(), val) { ValueType = node.Type });
                return expr;
            }


            /*非静态成员*/
            string memberName = node.Member.Name;
            LTSQLToken objToken = this.PopToken();
            if (objToken == null)
                throw new Exception($"表达式解析结果为null:{node}");

            //表名转换
            if (objToken is PrefixPropToken prefix)
            {
                TableAliasMapping mapping = prefix.AliasMapping.GetProp(memberName);
                //忽略掉 join 过程中的 属性前缀链
                if (string.IsNullOrEmpty(mapping.Alias))
                {
                    this.PushToken(new PrefixPropToken($"{prefix.ObjPrefix}.{memberName}") { ValueType = node.Type, AliasMapping = mapping });
                }
                // join 过程中的 属性前缀链 转化成表名
                else
                {
                    //外部对表达式树翻译
                    if (this.OnTranslateExpression(node, node.Type))
                        return expr;

                    this.PushToken(new AliasToken(mapping.Alias) { ValueType = node.Type });
                }
            }
            //字段访问
            else
            {
                //外部对表达式树翻译
                if (this.OnTranslateExpression(node, node.Type))
                    return expr;

                //对常量(静态对象)求值
                if (objToken is SqlParameterToken p)
                {
                    object obj = p.Value;
                    if (obj == null)
                        throw new Exception($"表达式不能依赖null对象求值：{obj}");

                    if (!this.OnTranslateMember(node.Member, obj, node.Expression.Type, node, node.Type, objToken, null))
                    {
                        //对象访问
                        object val = this.PropOrFieldValue(node.Member, obj);
                        this.PushToken(new SqlParameterToken(p.ParameterName, val) { ValueType = node.Type });
                    }
                }
                //非常量(表)
                else
                {
                    if (this.OnTranslateMember(node.Member, null, node.Expression.Type, node, node.Type, objToken, null))
                        return expr;

                    if (objToken is GroupObjToken groupToken && memberName == nameof(IGrouping<object, object>.Key))
                    {
                        //IGrouping.Key 的方位转换为分组依据
                        this.PushToken(groupToken.GroupKey);
                    }
                    else if(objToken is TupleToken tuple)
                    {
                        LTSQLToken prop = tuple.GetProp(memberName);
                        if (prop == null)
                            throw new Exception($"没有找到对应属性的解析结果, 表达式解析失败: {node}");

                        //对于元组的访问，转换为对应属性的token
                        this.PushToken(prop);
                    }
                    else
                    {
                        //对象访问
                        this.PushToken(new ObjectAccessToken(objToken, memberName) { ValueType = node.Type });
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
            if (this.OnTranslateExpression(node, node.Type))
                return expr;

            object val = null;
            LTSQLToken token = null;
            LTSQLToken objToken = null;
            LTSQLToken[] parameters = null;

            //参数列表
            parameters = this.PopAsParamters(node.Arguments.Count);

            //静态方法的调用
            if (node.Object == null)
            {
                //外部成员翻译
                if (this.OnTranslateMember(node.Method, null, node.Method.ReflectedType, node, node.Type, null, parameters))
                    return expr;

                //参数个数为0的静态方法，直接调用求值
                if (node.Arguments.Count == 0)
                {
                    val = node.Method.Invoke(null, null);
                    token = new SqlParameterToken(this._context.ParameterNameGenerator.Next(), val) { ValueType = node.Method.ReturnType };
                    this.PushToken(token);
                    return expr;
                }

                if (!parameters.All(p => p is SqlParameterToken))
                    throw new Exception($"静态方法引用动态参数值，无法继续转换：{node}");

                val = node.Method.Invoke(null, parameters.Select(p => ((SqlParameterToken)p).Value).ToArray());
                token = new SqlParameterToken(this._context.ParameterNameGenerator.Next(), val) { ValueType = node.Method.ReturnType };
                this.PushToken(token);
                return expr;
            }


            /* 实力方法调用*/
            MethodInfo method = node.Method;
            //实例对象
            objToken = this.PopToken();
            if (this.OnTranslateMember(node.Method, objToken is SqlParameterToken p ? p.Value : null, node.Object.Type, node, node.Type, objToken, parameters))
                return expr;

            //实例对象求值
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
                Parameters = parameters
            };
            this.PushToken(token);

            return expr;
        }
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            //if (this.OnTranslateExpression(node, node.Type))
            //    return node;

            LTSQLToken token = this.PeekToken();
            if (token is GroupObjToken groupToken)
            {
                //表示开始对分组对象的聚合函数作翻译，需要解析lambda表达式作为聚合函数的参数
                ParameterExpression parameter = node.Parameters[0];
                this.UseSpecialTokenInstead(parameter, this._context.GroupElement);
                this.Visit(node.Body);
                this.UnUseSpecialToken(parameter);
                return node;
            }

            //访问到lambda表达式，表示某些函数求值，其入参为lambda函数
            this.PushToken(new SqlParameterToken(this._context.ParameterNameGenerator.Next(), node) { ValueType = node.Type });
            return node;
        }
        //new 表达式
        protected override Expression VisitNew(NewExpression node)
        {
            Expression expr = base.VisitNew(node);
            if (this.OnTranslateExpression(node, node.Type))
                return expr;

            TupleToken tuple = new TupleToken();
            LTSQLToken[] paras = this.PopAsParamters(node.Arguments.Count);
            if (node.Members.IsNotEmpty())
            {
                for (int i = 0; i < node.Members.Count; i++)
                {
                    tuple.Add(paras[i], node.Members[i].Name);
                }
            }

            this.PushToken(tuple);
            return expr;
        }
        //初始化实例
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            Expression expr = base.VisitMemberInit(node); 
            if (this.OnTranslateExpression(node, node.Type))
                return expr;

            if (node.Bindings.Count > 0)
            {
                LTSQLToken[] bindProps = this.PopAsParamters(node.Bindings.Count);
                TupleToken tuple = this.PopToken() as TupleToken;

                tuple.ValueType = node.Type;
                for(int i = 0; i < node.Bindings.Count; i++)
                {
                    tuple.Add(bindProps[i], node.Bindings[i].Member.Name);
                }

                this.PushToken(tuple);
            }
            return expr;
        }
        //二元表达式
        protected override Expression VisitBinary(BinaryExpression node)
        {
            Expression expr = base.VisitBinary(node);
            if (this.OnTranslateExpression(node, node.Type))
                return expr;

            LTSQLToken right = this.PopToken();
            LTSQLToken left = this.PopToken();
            if (!(right is ValueToken && left is ValueToken))
                throw new Exception($"二元表达式左右两边的子节点无法正常表示:{node}");

            ValueToken vall = left as ValueToken;
            ValueToken valr = right as ValueToken;
            if (vall.ValueType != valr.ValueType)
                throw new Exception($"二元表达式左右两边的子节点求值后的类型不一致:{node}");

            if(node.NodeType == ExpressionType.Equal)
            {
                // join 的联表条件，可能会导致产元组条件
                if(vall is TupleToken tupl && valr is TupleToken tupr)
                {
                    if(tupl.Props.Length != tupr.Props.Length)
                        throw new Exception($"二元表达式左右两边的子节点求值后的类型不一致:{node}");
                    
                    //元组中的各个属性做相等操作，用AND操作连接
                    ConditionToken cur = null;
                    for(int i = 0; i < tupl.Props.Length; i++)
                    {
                        ConditionToken equals = new ConditionToken(tupl.Props[i], tupr.Props[i], "=");
                        cur = cur == null ? equals : new ConditionToken(cur, equals, "AND");
                    }

                    this.PushToken(new SqlScopeToken(cur));
                    return expr;
                }
            }

            SqlValueToken sqll = vall as SqlValueToken;
            SqlValueToken sqlr = valr as SqlValueToken;
            if (sqll == null || sqlr == null)
                throw new Exception($"二元表达式左右两边的子节点求值后的类型不一致:{node}");

            ConditionToken condition = null;
            switch (node.NodeType)
            {
                //case ExpressionType.Add:
                //    //c# 中的 字符串支持 "+" 号拼接操作，但在此处不做SQL字符串拼接函数的翻译操作，如果需要指定字符串拼接应该用 string.concat 函数操作
                //    condition = new ConditionToken(sqll, sqlr, "+");
                //    break;
                case ExpressionType.Equal:
                    condition = new ConditionToken(sqll, sqlr, ConditionToken.OPT_EQUAL);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    condition = new ConditionToken(sqll, sqlr, ConditionToken.OPT_GREATER_OR_EQUAL);
                    break;
                case ExpressionType.LessThanOrEqual:
                    condition = new ConditionToken(sqll, sqlr, ConditionToken.OPT_LESS_OR_EQUAL);
                    break;
                case ExpressionType.LessThan:
                    condition = new ConditionToken(sqll, sqlr, ConditionToken.OPT_LESS);
                    break;
                case ExpressionType.GreaterThan:
                    condition = new ConditionToken(sqll, sqlr, ConditionToken.OPT_GREATER);
                    break;
                case ExpressionType.AndAlso:
                    condition = new ConditionToken(sqll, sqlr, ConditionToken.OPT_AND);
                    break;
                case ExpressionType.OrElse:
                    condition = new ConditionToken(sqll, sqlr, ConditionToken.OPT_OR);
                    break;
                default:
                    throw new NotImplementedException($"暂不支持此二元表达式翻译：{node.NodeType}");
            }

            this.PushToken(new SqlScopeToken(condition));
            return expr;
        }
        //一元表达式：主要是取反操作，not exists 以及 not in 等
        protected override Expression VisitUnary(UnaryExpression node)
        {
            // not int 支持
            // not exists 支持

            Expression expr = base.VisitUnary(node);
            if (this.OnTranslateExpression(node, node.Type))
                return expr;

            LTSQLToken token = this.PopToken();
            if (token is ConditionToken condition)
                token = condition.Not();
            //else if (token is SQLScopeToken scope && scope.Inner is ConditionToken condition1)
            //{
                
            //}
            this.PushToken(token);
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
                    LTSQLTranslater = new CombineTranslaterSelector(options?.SQLTokenTranslaters, LTSQLTokenTranslaterSelector.Default)
                }
            });
        }
        internal LTSQLToken Translate(QuerySequence query, LTSQLScope scope)
        {
            query = query.UnWrap();

            this._scope = scope;
            this._context = scope.Context;
            this._context.Root = query; 
            this._tokens = new Stack<LTSQLToken>();

            return this.TranslateCore();
        }
    }
}
