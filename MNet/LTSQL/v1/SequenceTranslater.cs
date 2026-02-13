using MNet.LTSQL.v1.SqlTokens;
using MNet.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
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
            HashSet<string> objPrefixs = new HashSet<string>() { root };
            
            
            //涉及联表
            if (complex.From is FromJoinUnit join)
            {
                ParameterExpression joinObj = Expression.Parameter(((LambdaExpression)join.JoinExpr).Body.Type, root);
                this.AssignFromJoinAlias(join, param2table, objPrefixs, root, joinObj, joinObj);
            }
            //单表
            else
            {
                complex.From.Source.Alias = root;
                param2table.Add(root, root);
            }


            //统一根参数名
            ExpressionModifier exprModifier = new ExpressionModifier();
            ParameterExpression newParameter = Expression.Parameter(complex.Type, root);

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
                    list.Add(new KeyValuePair<Expression, bool>(newExpr, orderItem.Value));
                }

                complex.Order.OrderKeys = list;
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
        private void AssignFromJoinAlias(FromUnit from, Dictionary<string, string> param2table, HashSet<string> prefixs, string prefix, Expression obj, ParameterExpression root)
        {
            if (from is FromJoinUnit join)
            {
                LambdaExpression lamb1 = join.Source1Key as LambdaExpression;
                LambdaExpression lamb2 = join.Source2Key as LambdaExpression;

                Expression access1 = Expression.MakeMemberAccess(obj, obj.Type.GetMember(lamb1.Parameters[0].Name)[0]);
                Expression access2 = Expression.MakeMemberAccess(obj, obj.Type.GetMember(lamb2.Parameters[0].Name)[0]);

                string p1 = lamb1.Parameters[0].Name;
                string p2 = lamb2.Parameters[0].Name;
                if (!string.IsNullOrEmpty(prefix))
                {
                    p1 = $"{prefix}.{p1}";
                    p2 = $"{prefix}.{p2}";
                }

                prefixs.Add(p1);
                prefixs.Add(p2);

                this.AssignFromJoinAlias(join.From, param2table, prefixs, p1, access1, root);

                ExpressionModifier modifier = new ExpressionModifier();
                Expression joinKeyValue1 = modifier.VisitParameter(lamb1.Body, p => object.ReferenceEquals(p, lamb1.Parameters[0]) ? access1 : p);
                Expression joinKeyValue2 = modifier.VisitParameter(lamb2.Body, p => object.ReferenceEquals(p, lamb2.Parameters[0]) ? access2 : p);

                //叶结点
                param2table[p2] = this._context.TableNameGenerator.Next(); 
                join.Source.Alias = param2table[p2]; //生成表命名
                join.JoinOn = Expression.Lambda(Expression.Equal(joinKeyValue1, joinKeyValue2), root); //生成联表条件
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
                query = new SQLScopeToken(query);
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
        private GroupToken TranslateGroup(GroupUnit group)
        {
            this.Visit((group.GroupKeys as LambdaExpression).Body);

            GroupToken groupToken = new GroupToken();
            groupToken.GroupByItems = new List<LTSQLToken>();

            LTSQLToken token = this.PopToken();
            if (token is TupleToken tuple)
            {
                groupToken.GroupByItems.AddRange(tuple.Props);
            }
            else
            {
                groupToken.GroupByItems.Add(token);
            }

            return groupToken;
        }
        private OrderToken TranslateOrder(OrderUnit order)
        {
            if (order.OrderKeys.IsEmpty())
                return null;

            OrderToken orderToken = new OrderToken();
            orderToken.OrderByItems = new List<OrderByItemToken>();
            foreach (var orderKey in order.OrderKeys)
            {
                this.Visit((orderKey.Key as LambdaExpression).Body);
                LTSQLToken token = this.PopToken();
                orderToken.OrderByItems.Add(new OrderByItemToken() { IsAsc = !orderKey.Value, Item = token });
            }

            return orderToken;
        }
        private SelectToken TranslateSelect(SelectUnit select)
        {
            this.Visit((select.SelectKey as LambdaExpression).Body);
            LTSQLToken token = this.PopToken();

            SelectToken selectToken = new SelectToken();
            selectToken.Fields = new List<SelectItemToken>();
            if (token is TupleToken tuple)
            {
                selectToken.Fields.AddRange(tuple.Items.Select(p => new SelectItemToken(p.Item1, p.Item2)));
            }
            else
            {
                selectToken.Fields.Add(new SelectItemToken(token, null));
            }

            return selectToken;
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
            if(query.Group?.GroupKeys != null)
                queryToken.Group = this.TranslateGroup(query.Group);
            //order by
            if (query.Order?.OrderKeys.IsNotEmpty() ?? false)
                queryToken.Order = this.TranslateOrder(query.Order);
            //select
            if (query.Select?.SelectKey != null)
            {
                queryToken.Select = this.TranslateSelect(query.Select);
            }
            else
            {
                queryToken.Select = new SelectToken
                {
                    Fields = fields
                };
            }

            
            return queryToken;
        }


        //翻译参数
        protected override Expression VisitParameter(ParameterExpression node)
        {
            string objprefix = node.Name;
            if (this.IsObjectPrefix(objprefix))
            {
                //忽略掉join 过程中的 属性前缀链
                this.PushToken(new PrefixPropToken(objprefix)
                {
                    ValueType = node.Type
                });
            }
            else
            {
                TranslateContext ctx = this.NewTranslateContext();
                ctx.TranslateExpr = node;
                ctx.ExpressionValueType = node.Type;

                //外部转换优先
                if (!this.OnTranslateExpression(ctx))
                {
                    //默认转换
                    string tableName = this.GetTableName(objprefix);
                    if (tableName == null)
                        throw new Exception($"参数({objprefix})未找到表名:{node}");

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
            transCtx.ExpressionValueType = node.Type;

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
                transCtx1.ExpressionValueType = node.Type;
                if (this.OnTranslateExpression(transCtx1))
                    return expr;

                //外部对成员调用翻译
                var transCtx2 = this.NewTranslateContext();
                transCtx2.TranslateExpr = node;
                transCtx2.TranslateMember = node.Member;
                transCtx2.MemberOnwerType = node.Member.ReflectedType;
                transCtx2.ExpressionValueType = node.Type;
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
            if (token is PrefixPropToken prefix)
            {
                string accessPath = $"{prefix.ObjPrefix}.{memberName}";
                //忽略掉 join 过程中的 属性前缀链
                if (this.IsObjectPrefix(accessPath))
                {
                    this.PushToken(new PrefixPropToken(accessPath) { ValueType = node.Type });
                }
                // join 过程中的 属性前缀链 转化成表名
                else
                {
                    //外部对表达式树翻译
                    var transCtx1 = this.NewTranslateContext();
                    transCtx1.TranslateExpr = node;
                    transCtx1.ExpressionValueType = node.Type;
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
                transCtx1.ExpressionValueType = node.Type;
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
                    transCtx2.ExpressionValueType = node.Type;
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
                    transCtx2.ExpressionValueType = node.Type;
                    if (!this.OnTranslateMember(transCtx2))
                    {
                        //对象访问
                        this.PushToken(new ObjectAccessToken(token, memberName) { ValueType = node.Type });
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
            transCtx1.ExpressionValueType = node.Type;
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
                transCtx2.ExpressionValueType = node.Type;
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
                this.PushToken(token);
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
            transCtx3.ExpressionValueType = node.Type;
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
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            //访问到lambda表达式，表示某些函数求值，其入参为lambda函数
            this.PushToken(new SqlParameterToken(this._context.ParameterNameGenerator.Next(), node) { ValueType = node.Type });
            return node;
        }
        //new 表达式
        protected override Expression VisitNew(NewExpression node)
        {
            Expression expr = base.VisitNew(node);
            
            var transCtx = this.NewTranslateContext();
            transCtx.TranslateExpr = node;
            transCtx.ExpressionValueType = node.Type;
            if (this.OnTranslateExpression(transCtx))
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
        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            return base.VisitMemberBinding(node);
        }
        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            return base.VisitMemberMemberBinding(node);
        }
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            base.VisitMemberAssignment(node);
            return node; 
        }
        //初始化实例
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            Expression expr = base.VisitMemberInit(node); 

            var transCtx = this.NewTranslateContext();
            transCtx.TranslateExpr = node;
            transCtx.ExpressionValueType = node.Type;
            if (this.OnTranslateExpression(transCtx))
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

            var transCtx = this.NewTranslateContext();
            transCtx.TranslateExpr = node;
            transCtx.ExpressionValueType = node.Type;

            if (this.OnTranslateExpression(transCtx))
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

                    this.PushToken(new SQLScopeToken(cur));
                    return expr;
                }
            }

            SQLValueToken sqll = vall as SQLValueToken;
            SQLValueToken sqlr = valr as SQLValueToken;
            if (sqll == null || sqlr == null)
                throw new Exception($"二元表达式左右两边的子节点求值后的类型不一致:{node}");

            ConditionToken condition = null;
            switch (node.NodeType)
            {
                case ExpressionType.Add:
                    //c# 中的 字符串支持 "+" 号拼接操作，但在此处不做SQL字符串拼接函数的翻译操作，如果需要指定字符串拼接应该用 string.concat 函数操作
                    condition = new ConditionToken(sqll, sqlr, "+");
                    break;
                case ExpressionType.Equal:
                    condition = new ConditionToken(sqll, sqlr, "=");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    condition = new ConditionToken(sqll, sqlr, ">=");
                    break;
                case ExpressionType.LessThanOrEqual:
                    condition = new ConditionToken(sqll, sqlr, "<=");
                    break;
                case ExpressionType.LessThan:
                    condition = new ConditionToken(sqll, sqlr, "<");
                    break;
                case ExpressionType.GreaterThan:
                    condition = new ConditionToken(sqll, sqlr, ">");
                    break;
                case ExpressionType.AndAlso:
                    condition = new ConditionToken(sqll, sqlr, "AND");
                    break;
                case ExpressionType.OrElse:
                    condition = new ConditionToken(sqll, sqlr, "OR");
                    break;
                default:
                    throw new NotImplementedException($"暂不支持此二元表达式翻译：{node.NodeType}");
            }

            this.PushToken(new SQLScopeToken(condition));
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

            return this.TranslateCore();
        }
    }
}
