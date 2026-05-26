using MNet.LTSQL.Attributes;
using MNet.LTSQL.Objects;
using MNet.LTSQL.SqlQueryStructs;
using MNet.LTSQL.SqlTokenExtends;
using MNet.LTSQL.SqlTokens;
using MNet.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Diagnostics.Tracing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Linq;


namespace MNet.LTSQL
{
    //开启翻译
    public class SequenceTranslater : ExpressionVisitor, IQueryTranslater
    {
        public SequenceTranslater()
        { }


        private LTSQLTranslateScope _scope;
        private LTSQLContext _context;
        //生成的SQL令牌栈
        private Stack<LTSQLToken> _tokens;
        //复用对象，用于扩展翻译上下文，避免频繁创建对象
        private TranslateContext _templateContext;
        private Stack<(Expression expr, LTSQLToken token)> _bufferLayer;

        private static string GetTableName(LTSQLMemberContext ctx)
        {
            if (ctx.Owner == null)
                throw new Exception("表名称获取异常， 未传入实体类型，无法获取。");

            TableAttribute attr = ctx.Owner.GetCustomAttribute<TableAttribute>();
            return attr?.Name ?? ctx.Owner.Name;
        }
        private static string GetColumnName(LTSQLMemberContext ctx)
        {
            if (ctx.Member == null)
                throw new Exception("表字段获取异常， 未传入属性或者字段信息，无法获取。");

            ColumnAttribute attr = ctx.Member.GetCustomAttribute<ColumnAttribute>();
            return attr?.Name ?? ctx.Member.Name;
        }

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
        private TableDescriptor GetRootTableAliasMapping(string parameterName)
        {
            LTSQLContext context = this._context;
            LTSQLTranslateScope scope = this._scope;
            TableDescriptor ret = null;

            while (true)
            {
                ret = context.TableRefs?.GetTableRef(parameterName);
                bool flag = ret != null; ;
                if (flag)
                    break;

                if (scope.Parent == null || scope.Parent.Context == null)
                    throw new Exception($"参数名({parameterName})无法找到对应的上下文作用域, 无法解析表命名");

                scope = scope.Parent;
                context = scope.Context;
            }

            return ret;
        }
        private void UnUseSpecialToken()
        {
            this._bufferLayer.Pop();
        }
        private void UseToken(Expression expr, LTSQLToken token)
        {
            this._bufferLayer.Push((expr, token));
        }
        private LTSQLToken PopParameterToken(Expression expr)
        {
            if (this._bufferLayer.Count > 0)
            {
                //if (this._bufferLayer.Peek().expr == expr)
                //    return this._bufferLayer.Peek().token;
                foreach (var item in this._bufferLayer)
                {
                    if (item.expr == expr)
                        return item.token;
                }
            }

            return null;
        }

        //递归分配表命名并统一命名
        private void AssignTableAlias(ref string root)
        {
            SqlQueryPart query = this._context.Root;
            if (query == null)
                return;

            bool joinSelectFlag = query.Step == QueryStepSeq.Join; //join select: join之后没有后续操作，join的结果既是 select 结果
            root = "root_" + this._context.TableAliasGenerator.Next();
            ExpressionModifier exprModifier = new ExpressionModifier();

            //统一根参数名(存在select 字段硬编码查询)
            if (query.Wheres.IsNotEmpty())
            {
                //where 多条件合并
                Expression merge = null;
                ParameterExpression _old = query.Wheres[0].AsLambda().TakeParamter(0);
                ParameterExpression _new = Expression.Parameter(_old.Type, root);
                foreach (Expression expr in query.Wheres)
                {
                    LambdaExpression lambda = expr.AsLambda();
                    Expression newExpr = exprModifier.ModifyParameter(lambda.Body, lambda.TakeParamter(0), _new);
                    merge = merge == null ? newExpr : Expression.AndAlso(merge, newExpr);
                }

                query.Wheres.Clear();
                query.Wheres.Add(Expression.Lambda(merge, _new));
            }

            // group by
            if (query.GroupFlag)
            {
                ParameterExpression _old = null;
                ParameterExpression _new = null;
                if (query.GroupKey != null)
                {
                    _old = query.GroupKey.AsLambda().TakeParamter(0);
                    _new = Expression.Parameter(_old.Type, root);
                    query.GroupKey = exprModifier.ModifyParameter(query.GroupKey, _old, _new);
                }

                if (query.GroupElement != null)
                {
                    _old = query.GroupElement.AsLambda().TakeParamter(0);
                    _new = Expression.Parameter(_old.Type, root);
                    query.GroupElement = exprModifier.ModifyParameter(query.GroupElement, _old, _new);
                }
            }

            // having
            if (query.Havings.IsNotEmpty())
            {
                //多条件合并
                Expression merge = null;
                ParameterExpression _old = query.Havings[0].AsLambda().TakeParamter(0);
                ParameterExpression _new = Expression.Parameter(_old.Type, root);
                foreach (Expression expr in query.Havings)
                {
                    LambdaExpression lambda = expr.AsLambda();
                    Expression newExpr = exprModifier.ModifyParameter(lambda.Body, lambda.TakeParamter(0), _new);
                    merge = merge == null ? newExpr : Expression.AndAlso(merge, newExpr);
                }

                query.Havings.Clear();
                query.Havings.Add(Expression.Lambda(merge, _new));
            }

            //排序（仅在不存在分组的情况下才有替换参数的意义）
            if (query.Orders.IsNotEmpty() && !query.GroupFlag)
            {
                ParameterExpression _old = query.Orders[0].Key.AsLambda().TakeParamter(0);
                ParameterExpression _new = Expression.Parameter(_old.Type, root);
                foreach (var orderItem in query.Orders)
                {
                    LambdaExpression lambda = orderItem.Key.AsLambda();
                    orderItem.Key = exprModifier.ModifyParameter(lambda, lambda.TakeParamter(0), _new);
                }
            }

            //投影（仅在不存在分组的情况下才有替换参数的意义, 并且只有在非join select的情况下才会存在统一参数）
            if (query.SelectKey != null && !query.GroupFlag && !joinSelectFlag)
            {
                LambdaExpression lambda = query.SelectKey.AsLambda();
                ParameterExpression _old = lambda.TakeParamter(0);
                ParameterExpression _new = Expression.Parameter(_old.Type, root);
                query.SelectKey = exprModifier.ModifyParameter(lambda, _old, _new);
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
        private string OnGetTableName(Type owner, string alias)
        {
            LTSQLMemberContext memberCtx = new LTSQLMemberContext();
            memberCtx.Owner = owner;
            memberCtx.OwnerName = alias;
            return this._context.Options.GetTableName(memberCtx);
        }
        private string OnGetColumnName(Type owner, string alias, MemberInfo member)
        {
            LTSQLMemberContext memberCtx = new LTSQLMemberContext();
            memberCtx.Owner = owner;
            memberCtx.OwnerName = alias;
            memberCtx.Member = member;
            return this._context.Options.GetColumnName(memberCtx);
        }


        private LTSQLToken TranslateLambda(LambdaExpression lambda, params LTSQLToken[] rets)
        {
            if(rets.IsNotEmpty() && rets.Length > lambda.Parameters.Count)
                throw new Exception("替换参数个数大于实际的参数个数");

            var paras = lambda.Parameters.ToArray();
            int len = Math.Min(rets?.Length ?? 0, paras?.Length ?? 0);
            List<(Expression expr, LTSQLToken ret)> list = new List<(Expression expr, LTSQLToken ret)>();
            for(int i = 0; i < len; i++)
            {
                list.Add((paras[i], rets[i]));
            }

            return this.Translate(lambda.Body, list.ToArray());
        }
        private LTSQLToken Translate(Expression exprs, params (Expression expr, LTSQLToken ret)[] list)
        {
            if(list.Length > 0)
            {
                foreach ((Expression expr, LTSQLToken ret) in list)
                {
                    this.UseToken(expr, ret);
                }
            }

            try
            {
                this.Visit(exprs);
                return this.PopToken();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (list.Length > 0)
                {
                    foreach ((Expression expr, LTSQLToken ret) in list)
                    {
                        this.UnUseSpecialToken();
                    }
                }
            }
        }
        

        private LTSQLToken TranslateFrom(QueryPart from, string root, ref TableRefs refs, out Expression fromObj)
        {
            return TranslateQueryPart(from, root, ref refs, out fromObj);
        }
        private LTSQLToken TranslateQueryPart(QueryPart from, string parameterName, ref TableRefs refs, out Expression fromObj)
        {
            LTSQLToken src = null;
            string tableAlias = null;
            fromObj = null;

            if (from is JoinPart join)
            {
                fromObj = join.JoinObject;

                LTSQLToken query1 = this.TranslateQueryPart(join.MainQuery, join.JoinObject.AsLambda().TakeParamter(0).Name, ref refs, out _);
                LTSQLToken query2 = this.TranslateQueryPart(join.JoinQuery, join.JoinObject.AsLambda().TakeParamter(1).Name, ref refs, out _);

                //透明表
                TableDescriptor descriptor = new TableDescriptor(parameterName, true);
                descriptor.Alias = parameterName;
                descriptor.AddField(new FieldDescriptor(join.JoinObject.AsLambda().TakeParamter(0).Name, query1, join.MainQuery.MappingType));
                descriptor.AddField(new FieldDescriptor(join.JoinObject.AsLambda().TakeParamter(1).Name, query1, join.JoinQuery.MappingType));
                refs.AddTableRef(parameterName, descriptor);

                //连接查询
                //合并查询
                if (join.JoinKey1 != null)
                {
                    LambdaExpression expr1 = join.JoinKey1.AsLambda();
                    LambdaExpression expr2 = join.JoinKey2.AsLambda();
                    LambdaExpression expr3 = Expression.Lambda(Expression.Equal(expr1.Body, expr2.Body), expr1.TakeParamter(0), expr2.TakeParamter(0));
                    LTSQLToken joinKeys = this.TranslateLambda(expr3, new PrefixPropToken("", refs));
                    JoinToken joinToken = new JoinToken(join.JoinType, query1, query2, joinKeys);
                    return joinToken;
                }
                else
                {
                    //from 中的内联接查询
                    //拆包，使其在同一范围内
                    if (query1 is ListToken list)
                        return LTSQLTokenFactory.CreateListToken(list.Tokens, query2);
                    return LTSQLTokenFactory.CreateListToken(query1, query2);
                }
            }
            else if (from is TablePart table)
            {
                tableAlias = this._context.TableAliasGenerator.Next();
                string tableName = table.TableName ?? this.OnGetTableName(table.MappingType, tableAlias);
                
                LTSQLToken qry = LTSQLTokenFactory.CreateTableObjectToken(tableName, table.MappingType);
                TableDescriptor descriptor = new TableDescriptor(tableName, tableAlias, table.MappingType);
                descriptor.Alias = tableAlias;

                //解析属性
                foreach (PropertyInfo prop in table.MappingType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (prop.IsDefined(typeof(NonFiledAttribute)))
                        continue;

                    string fieldName = this.OnGetColumnName(table.MappingType, table.Alias, prop);
                    LTSQLToken fieldAccess = LTSQLTokenFactory.CreateAccessToken(
                        LTSQLTokenFactory.CreateTableObjectToken(tableAlias, table.MappingType), fieldName, prop.PropertyType
                        );

                    descriptor.AddField(new FieldDescriptor(prop.Name, fieldAccess, prop.PropertyType));
                }
                //解析字段
                foreach (FieldInfo prop in table.MappingType.GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (prop.IsDefined(typeof(NonFiledAttribute)))
                        continue;

                    string fieldName = this.OnGetColumnName(table.MappingType, table.Alias, prop);
                    LTSQLToken fieldAccess = LTSQLTokenFactory.CreateAccessToken(
                        LTSQLTokenFactory.CreateTableObjectToken(tableAlias, table.MappingType), fieldName, prop.FieldType
                        );

                    descriptor.AddField(new FieldDescriptor(prop.Name, fieldAccess, prop.FieldType));
                }

                src = qry;
                refs.AddTableRef(parameterName, descriptor);
            }
            else
            {
                IQueryTranslater translater = new QueryTranslaterFactory().Create(from);
                if (translater == null)
                    throw new Exception($"不支持的查询结构:{from.GetType().FullName}");

                LTSQLToken qry = src = translater.Translate(from, this._scope.NewScope());
                //解析字段
                if (qry is ISelectable select)
                {
                    tableAlias = this._context.TableAliasGenerator.Next();
                    TableDescriptor descriptor = new TableDescriptor("$temp", tableAlias, select.MappingType);
                    descriptor.Alias = tableAlias;
                    foreach ((string key, LTSQLToken val) in select)
                    {
                        string fieldAlias = key ?? "field";
                        LTSQLToken fieldAccess = LTSQLTokenFactory.CreateAccessToken(
                                LTSQLTokenFactory.CreateTableObjectToken(tableAlias, select.MappingType),
                                fieldAlias,
                                select.GetValueType(key)
                        );

                        descriptor.AddField(new FieldDescriptor(fieldAlias, fieldAccess, select.GetValueType(key)));
                    }
                    refs.AddTableRef(parameterName, descriptor);
                }

                qry = LTSQLTokenFactory.CreatePriorityCalcToken(qry);
                src = qry;
            }

            return LTSQLTokenFactory.CreateAliasToken(src, tableAlias);
        }

        private LTSQLToken TranslateWhere(LambdaExpression wheres, LTSQLToken parameter)
        {
            if (wheres == null)
                return null;

            LTSQLToken token = this.TranslateLambda(wheres, parameter);

            return token;
        }
        private LTSQLToken TranslateGroup(LambdaExpression groupKey, LambdaExpression groupEle, LTSQLToken parameters, out GroupObjToken groupToken)
        {
            groupToken = null;
            LTSQLToken groupKeyToken = null;
            LTSQLToken groupEleToken = null;
            List<LTSQLToken> groupKeyTokens = new List<LTSQLToken>();

            //分组元素翻译
            if (groupEle != null)
                groupEleToken = this.TranslateLambda(groupEle, parameters);

            //分组依据翻译
            if (groupKey != null)
            {
                groupKeyToken = this.TranslateLambda(groupKey, parameters);

                if (groupKeyToken is TupleToken tuple)
                    groupKeyTokens.AddRange(tuple.Props.ToArray());
                else
                    groupKeyTokens.Add(groupKeyToken);
            }

            groupToken = new GroupObjToken(groupEleToken, groupKeyToken);
            if (groupKeyTokens.Count <= 0)
                return null; //不是直接group by操作，可能是直接做单一的聚合查询，如 select count(1) from xxx

            return LTSQLTokenFactory.CreateListToken(groupKeyTokens.ToArray());
        }
        private LTSQLToken TranslateHaving(LambdaExpression havings, LTSQLToken parameters)
        {
            if (havings == null)
                return null;

            try
            {
                LTSQLToken token = this.TranslateLambda(havings, parameters);
                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
        private LTSQLToken TranslateOrder(List<OrderKeyPart> orders, LTSQLToken parameters)
        {
            if (orders.IsEmpty())
                return null;

            bool group = this._context.Root.GroupFlag;
            List<LTSQLToken> orderKeyTokens = new List<LTSQLToken>();
            foreach (OrderKeyPart getKey in orders)
            {
                LambdaExpression lambda = getKey.Key.AsLambda();
                LTSQLToken token = this.TranslateLambda(lambda, parameters);
                orderKeyTokens.Add(
                    SequenceToken.Create(
                        token,
                        SyntaxToken.Create(" "),
                        SyntaxToken.Create(getKey.Asc ? "ASC" : "DESC")
                        )
                    );
            }

            return LTSQLTokenFactory.CreateListToken(orderKeyTokens.ToArray());
        }
        private LTSQLToken TranslateSelect(LambdaExpression selectKey, LTSQLToken parameters, ref TableDescriptor descriptor)
        {
            bool group = this._context.Root.GroupFlag;

            try
            {
                LTSQLToken[] parameterObjs = new LTSQLToken[selectKey.Parameters.Count];
                Array.Fill(parameterObjs, parameters);

                TupleToken selectNew = new TupleToken(selectKey.ReturnType);
                LTSQLToken token = this.TranslateLambda(selectKey, parameterObjs);
                List<LTSQLToken> fields = new List<LTSQLToken>();
                if (token is TupleToken tuple)
                {
                    fields.AddRange(tuple.Select(p => LTSQLTokenFactory.CreateAliasToken(p.Item2, p.Item1)));

                    foreach ((string key, LTSQLToken val) in tuple)
                    {
                        descriptor.AddField(new FieldDescriptor(key, val, tuple.GetValueType(key)));
                    }
                }
                else if (token is ObjectAccessToken access)
                {
                    fields.Add(LTSQLTokenFactory.CreateAliasToken(access, access.Prop));
                    descriptor.AddField(new FieldDescriptor(access.Prop, access, access.ValueType));
                }
                else
                {
                    fields.Add(LTSQLTokenFactory.CreateAliasToken(token, "field"));
                    descriptor.AddField(new FieldDescriptor("field", token, selectKey.ReturnType));
                }
                
                return LTSQLTokenFactory.CreateListToken(fields.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
        //开始翻译
        private SqlQueryToken TranslateCore()
        {
            string root = null;
            this.AssignTableAlias(ref root);

            TableRefs refs = new TableRefs();
            SqlQueryPart query = this._context.Root;
            SqlQueryToken sqlToken = new SqlQueryToken();
            LTSQLToken parameterObj = null;
            Expression fromObj = null;

            sqlToken.Table = new TableDescriptor("$self", null, query.MappingType);
            this._context.TableRefs = refs;

            //from
            if (query.From != null)
            {
                sqlToken.From = LTSQLTokenFactory.CreateClauseToken("FROM", this.TranslateFrom(query.From, root, ref refs, out fromObj));    
            }

            parameterObj = new PrefixPropToken("", refs);

            //where
            if (query.Wheres.IsNotEmpty())
            {
                LTSQLToken condition = this.TranslateWhere(query.Wheres[0].AsLambda(), parameterObj);
                sqlToken.Where = LTSQLTokenFactory.CreateClauseToken("WHERE", condition);
            }

            //group by
            if (query.GroupFlag)
            {
                LambdaExpression lambda1 = query.GroupKey.AsLambda();
                LambdaExpression lambda2 = query.GroupElement.AsLambda();

                LTSQLToken groupKeys = this.TranslateGroup(lambda1, lambda2, parameterObj, out GroupObjToken groupObj);
                if (groupKeys != null)
                    sqlToken.Group = LTSQLTokenFactory.CreateClauseToken("GROUP BY", groupKeys);

                parameterObj = groupObj;
            }

            //having
            if (query.Havings.IsNotEmpty())
            {
                LTSQLToken condition = this.TranslateHaving(query.Havings[0].AsLambda(), parameterObj);
                sqlToken.Having = LTSQLTokenFactory.CreateClauseToken("HAVING", condition);
            }

            //order by
            if (query.Orders.IsNotEmpty())
            {
                LTSQLToken orderKeys = this.TranslateOrder(query.Orders, parameterObj);
                sqlToken.Order = LTSQLTokenFactory.CreateClauseToken("ORDER BY", orderKeys);
            }

            //select
            LTSQLToken selectFieldsToken = null;
            if (query.SelectKey != null || fromObj != null)
            {
                TableDescriptor descriptor = sqlToken.Table;
                Expression selectExpr = query.SelectKey ?? fromObj;
                selectFieldsToken = this.TranslateSelect(selectExpr.AsLambda(), parameterObj, ref descriptor);
                sqlToken.Table = descriptor;
            }

            else
            {

                sqlToken.Table = refs.BuildNewMeger(sqlToken.Table.Alias);
                selectFieldsToken = LTSQLTokenFactory.CreateListToken(sqlToken.Table.Select(p => p.value).ToArray());
            }

            //distict
            LTSQLToken distinckClause = null;
            if (query.Distinct)
                distinckClause = LTSQLTokenFactory.CreateClauseToken("DISTINCT");

            //分页
            LTSQLToken topLimitClause = null;
            if (query.Skip != null || query.Take != null)
            {
                if (query.Skip == null && this._context.Options?.DbType == DbTypes.MSSQL)
                {
                    //sql server 的 top 语法
                    topLimitClause = LTSQLTokenFactory.CreateClauseToken("TOP",
                        LTSQLTokenFactory.CreateSqlParameterToken(this._context.ParameterNameGenerator.Next(), query.Take, typeof(int))
                    );
                }
                else
                {
                    sqlToken.Page = LTSQLTokenFactory.CreatePageToken(query.Skip, query.Take);
                }
            }

            sqlToken.Select = LTSQLTokenFactory.CreateClauseToken("SELECT",
                new[] { distinckClause, topLimitClause, selectFieldsToken }.Where(p => p != null).ToArray()
            );
            sqlToken.ValueType = typeof(ILTSQLObjectQueryable<>).MakeGenericType(query.MappingType);


            //内联查询翻译
            sqlToken = LTSQLTokenVisitor.Visit(sqlToken, (t) =>
            {
                //如果存在内联查询，需要进一步翻译
                if (t is SqlParameterToken p)
                {
                    if (p.Value is ILTSQLObjectQueryable subquery)
                    {
                        LTSQLToken subQueryToken = new SequenceTranslater()
                       .Translate(subquery.Query, this._scope.NewScope());
                        return LTSQLTokenFactory.CreatePriorityCalcToken(subQueryToken as SqlQueryToken);
                    }
                }
                return t;
            }) as SqlQueryToken;

            //子查询，优先级运算处理(sqllite不支持多余的括号，所以需要处理)
            sqlToken = LTSQLTokenVisitor.Visit(sqlToken, t =>
            {
                if (t is FunctionCallToken c && c.FunctionObject is ObjectToken f && f.Alias == SqlFunctionHelper.F_EXISTS)
                {
                    LTSQLToken parameter = c.Parameters[0];
                    FunctionCallToken fcall = SqlFunctionHelper.ExistsFunction(this._context.Options.DbType, parameter.UnPriorityIfSubQuery())
                    .Build() as FunctionCallToken;
                    return c.IsNot ? fcall.Not() : fcall;
                }
                return t;
            }) as SqlQueryToken;

            //null 等式处理
            if (!this._context.Options.DisNullable)
            {
                sqlToken = LTSQLTokenVisitor.Visit(sqlToken, (t) =>
                {
                    if (t is SqlParameterToken p && p.Value == null)
                        return LTSQLTokenFactory.CreateNullToken(p.ValueType, this._context.Options.DbType);
                    return t;
                }) as SqlQueryToken;

                sqlToken = LTSQLTokenVisitor.Visit(sqlToken, (t) =>
                {
                    if (t is BoolCalcToken cdt && (cdt.Opration == BoolCalcToken.OPT_EQUAL || cdt.Opration == BoolCalcToken.OPT_NOT_EQUAL))
                    {
                        string opt = cdt.Opration == BoolCalcToken.OPT_EQUAL ? BoolCalcToken.OPT_IS : BoolCalcToken.OPT_IS_NOT;

                        if (cdt.Left is NullToken)
                            return LTSQLTokenFactory.CreateBoolCalcToken(opt, cdt.Right, cdt.Left);
                        else if (cdt.Right is NullToken)
                            return LTSQLTokenFactory.CreateBoolCalcToken(opt, cdt.Left, cdt.Right);
                    }
                    return t;
                }) as SqlQueryToken;
            }

            return sqlToken;
        }



        //翻译参数
        protected override Expression VisitParameter(ParameterExpression node)
        {
            LTSQLToken token = this.PopParameterToken(node);
            PrefixPropToken prefix = token as PrefixPropToken;
            if (prefix != null)
            {
                TableDescriptor tableDescriptor = prefix.TableRefs.GetTableRef(node.Name);
                if (tableDescriptor == null)
                    throw new Exception($"无法解析参数节点：{node}");

                if (tableDescriptor.IsHide)
                    this.PushToken(new PrefixPropToken(node.Name, prefix.TableRefs));
                else
                    this.PushToken(LTSQLTokenFactory.CreateTableObjectToken(tableDescriptor.Alias, node.Type));
            }
            else if (token != null)
            {
                //外部转换优先
                if (!this.OnTranslateExpression(node, node.Type))
                    this.PushToken(token);
            }
            else
            {
                //外部转换优先
                if (!this.OnTranslateExpression(node, node.Type))
                {
                    //确定参数范围
                    TableDescriptor tableDescriptor = this.GetRootTableAliasMapping(node.Name);
                    if (tableDescriptor == null)
                        throw new Exception($"无法解析参数节点：{node}");

                    if (tableDescriptor.IsHide)
                        this.PushToken(new PrefixPropToken(node.Name, this._context.TableRefs));
                    else
                        this.PushToken(LTSQLTokenFactory.CreateTableObjectToken(tableDescriptor.Alias, node.Type));
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

            this.PushToken(LTSQLTokenFactory.CreateSqlParameterToken(this._context.ParameterNameGenerator.Next(), node.Value, node.Type));
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
                this.PushToken(LTSQLTokenFactory.CreateSqlParameterToken(this._context.ParameterNameGenerator.Next(), val, node.Type));
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
                TableDescriptor descriptor = prefix.TableRefs.GetTableRef(memberName);
                if (descriptor == null)
                    throw new Exception($"无法解析成员：{memberName}. ([{node}])");

                //忽略掉 join 过程中的 属性前缀链
                if (descriptor.IsHide)
                {
                    this.PushToken(new PrefixPropToken($"{prefix.ObjPrefix}.{memberName}", prefix.TableRefs));
                }
                // join 过程中的 属性前缀链 转化成表名
                else
                {
                    //外部对表达式树翻译
                    if (this.OnTranslateExpression(node, node.Type))
                        return expr;

                    this.PushToken(LTSQLTokenFactory.CreateTableObjectToken(descriptor.Alias, node.Type));
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
                        this.PushToken(LTSQLTokenFactory.CreateSqlParameterToken(p.ParameterName, val, node.Type));
                    }
                }
                //非常量(表)
                else
                {
                    if (this.OnTranslateMember(node.Member, null, node.Expression.Type, node, node.Type, objToken, null))
                        return expr;

                    if (objToken is GroupObjToken groupToken && memberName == nameof(IGrouping<object, object>.Key))
                    {
                        //IGrouping.Key 的访问转换为分组依据元组的访问
                        this.PushToken(groupToken.GroupKey);
                    }
                    else if (objToken is TupleToken tuple)
                    {
                        //string fieldName = this.OnGetColumnName(tuple.ValueType, null, node.Member);
                        LTSQLToken prop = tuple.GetValue(memberName);
                        if (prop == null)
                            throw new Exception($"没有找到对应属性的解析结果, 表达式解析失败: {node}");

                        //对于元组的访问，转换为对应属性的token
                        this.PushToken(prop);
                    }
                    else
                    {
                        //对象访问
                        string fieldName = this.OnGetColumnName((objToken as ObjectToken)?.ValueType, (objToken as ObjectToken)?.Alias, node.Member);
                        this.PushToken(LTSQLTokenFactory.CreateAccessToken(objToken, fieldName, node.Type));
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
                    token = LTSQLTokenFactory.CreateSqlParameterToken(this._context.ParameterNameGenerator.Next(), val, node.Method.ReturnType);
                    this.PushToken(token);
                    return expr;
                }

                if (!parameters.All(p => p is SqlParameterToken))
                    throw new Exception($"静态方法引用动态参数值，无法继续转换：{node}");

                val = node.Method.Invoke(null, parameters.Select(p => ((SqlParameterToken)p).Value).ToArray());
                token = LTSQLTokenFactory.CreateSqlParameterToken(this._context.ParameterNameGenerator.Next(), val, node.Method.ReturnType);
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
                token = LTSQLTokenFactory.CreateSqlParameterToken(this._context.ParameterNameGenerator.Next(), val, node.Method.ReturnType);
                this.PushToken(token);
                return expr;
            }

            //sql 函数调用
            token = LTSQLTokenFactory.CreateFunctionCallToken(node.Method.Name, parameters, node.Method.ReturnType);
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
                LTSQLToken ret = this.TranslateLambda(node.AsLambda(), groupToken.Element);
                this.PushToken(ret);
                return node;
            }

            //访问到lambda表达式，表示某些函数求值，其入参为lambda函数
            this.PushToken(LTSQLTokenFactory.CreateSqlParameterToken(this._context.ParameterNameGenerator.Next(), node, node.Type));
            return node;
        }
        //new 表达式
        protected override Expression VisitNew(NewExpression node)
        {
            Expression expr = base.VisitNew(node);
            if (this.OnTranslateExpression(node, node.Type))
                return expr;

            TupleToken tuple = new TupleToken(node.Type);
            LTSQLToken[] paras = this.PopAsParamters(node.Arguments.Count);
            if (node.Members.IsNotEmpty())
            {
                for (int i = 0; i < node.Members.Count; i++)
                {
                    PropertyInfo prop = node.Members[i] as PropertyInfo;
                    FieldInfo field = node.Members[i] as FieldInfo;
                    tuple.Add(node.Members[i].Name, paras[i], prop?.PropertyType ?? field.FieldType);
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
                for (int i = 0; i < node.Bindings.Count; i++)
                {
                    PropertyInfo prop = node.Bindings[i].Member as PropertyInfo;
                    FieldInfo field = node.Bindings[i].Member as FieldInfo;
                    tuple.Add(node.Bindings[i].Member.Name, bindProps[i], prop?.PropertyType ?? field.FieldType);
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

            LTSQLToken right = this.PopToken().PriorityIfSubQuery();
            LTSQLToken left = this.PopToken().PriorityIfSubQuery();
            if (!(right is ValueToken && left is ValueToken))
                throw new Exception($"二元表达式左右两边的子节点无法正常表示:{node}");

            ValueToken vall = left as ValueToken;
            ValueToken valr = right as ValueToken;

            //理论上也不需要验证类型是否相等，因为编译编译通过了就证明类型能够相互转换了
            if (vall.ValueType != valr.ValueType)
            {
                //对可空类型的检验支持，如：int? 与 int 是相等的
                Type nullable = typeof(Nullable<>);
                bool flag1 = vall.ValueType.IsGenericType && (vall.ValueType.GetGenericTypeDefinition() == nullable);
                bool flag2 = valr.ValueType.IsGenericType && (valr.ValueType.GetGenericTypeDefinition() == nullable);
                bool flag3 = !flag1 && !flag2; //是否需要异常
                if (!flag3)
                {
                    Type selfType = null;
                    Type argsType = null;
                    if (flag1)
                    {
                        selfType = valr.ValueType;
                        argsType = vall.ValueType.GetGenericArguments()[0];
                    }
                    else
                    {
                        selfType = vall.ValueType;
                        argsType = valr.ValueType.GetGenericArguments()[0];
                    }
                    flag3 = selfType != argsType;
                }
                if (flag3)
                    throw new Exception($"二元表达式左右两边的子节点求值后的类型不一致:{node}");
            }

            if (node.NodeType == ExpressionType.Equal)
            {
                // join 的联表条件，可能会导致产元组条件
                if (vall is TupleToken tupl && valr is TupleToken tupr)
                {
                    if (tupl.Props.Length != tupr.Props.Length)
                        throw new Exception($"二元表达式左右两边的子节点求值后的类型不一致:{node}");

                    //元组中的各个属性做相等操作，用AND操作连接（join 操作会出现元组对比）
                    BoolCalcToken cur = null;
                    for (int i = 0; i < tupl.Props.Length; i++)
                    {
                        BoolCalcToken equals = LTSQLTokenFactory.CreateBoolCalcToken("=", tupl.Props[i], tupr.Props[i]);
                        cur = cur == null ? equals : LTSQLTokenFactory.CreateBoolCalcToken("AND", cur, equals);
                    }

                    this.PushToken(LTSQLTokenFactory.CreatePriorityCalcToken(cur));
                    return expr;
                }
            }

            SqlValueToken sqll = vall as SqlValueToken;
            SqlValueToken sqlr = valr as SqlValueToken;
            if (sqll == null || sqlr == null)
                throw new Exception($"二元表达式左右两边的子节点求值后的类型不一致:{node}");

            LTSQLToken binary = null;
            switch (node.NodeType)
            {
                case ExpressionType.Add:
                    binary = LTSQLTokenFactory.CreateAdd(sqll, sqlr, node.Type);
                    break;
                case ExpressionType.Subtract:
                    binary = LTSQLTokenFactory.CreateSubtract(sqll, sqlr, node.Type);
                    break;
                case ExpressionType.Divide:
                    binary = LTSQLTokenFactory.CreateDivide(sqll, sqlr, node.Type);
                    break;
                case ExpressionType.Multiply:
                    binary = LTSQLTokenFactory.CreateMultiply(sqll, sqlr, node.Type);
                    break;
                case ExpressionType.Equal:
                    binary = LTSQLTokenFactory.CreateBoolCalcToken(BinaryToken.OPT_EQUAL, sqll, sqlr);
                    break;
                case ExpressionType.NotEqual:
                    binary = LTSQLTokenFactory.CreateBoolCalcToken(BinaryToken.OPT_NOT_EQUAL, sqll, sqlr);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    binary = LTSQLTokenFactory.CreateBoolCalcToken(BinaryToken.OPT_GREATER_OR_EQUAL, sqll, sqlr);
                    break;
                case ExpressionType.LessThanOrEqual:
                    binary = LTSQLTokenFactory.CreateBoolCalcToken(BinaryToken.OPT_LESS_OR_EQUAL, sqll, sqlr);
                    break;
                case ExpressionType.LessThan:
                    binary = LTSQLTokenFactory.CreateBoolCalcToken(BinaryToken.OPT_LESS, sqll, sqlr);
                    break;
                case ExpressionType.GreaterThan:
                    binary = LTSQLTokenFactory.CreateBoolCalcToken(BinaryToken.OPT_GREATER, sqll, sqlr);
                    break;
                case ExpressionType.AndAlso:
                    binary = LTSQLTokenFactory.CreateBoolCalcToken(BoolCalcToken.OPT_AND, sqll, sqlr);
                    break;
                case ExpressionType.OrElse:
                    binary = LTSQLTokenFactory.CreateBoolCalcToken(BoolCalcToken.OPT_OR, sqll, sqlr);
                    break;
                case ExpressionType.Coalesce:
                    {
                        //空值合并符处理： a ?? b
                        binary = SqlFunctionHelper.CoalesceFunction(this._context.Options.DbType, node.Type, sqll, sqlr)
                                .Build();
                        break;
                    }
                default:
                    throw new NotImplementedException($"暂不支持此二元表达式翻译：{node.NodeType}");
            }

            if (binary != null)
                this.PushToken(LTSQLTokenFactory.CreatePriorityCalcToken(binary));
            return expr;
        }
        //一元表达式：主要是取反操作，not exists 以及 not in 等
        protected override Expression VisitUnary(UnaryExpression node)
        {
            // not int 支持
            // not exists 支持
            Expression expr = base.VisitUnary(node);

            // (int?)val; 类型转换也是一元表达式，需要过滤下
            if (node.NodeType == ExpressionType.Convert)
            {
                //类型转换
                LTSQLToken value = this.PopToken();
                if (value is SqlParameterToken p)
                {
                    p = LTSQLTokenFactory.CreateSqlParameterToken(p.ParameterName, Convert.ChangeType(p.Value, node.Type), node.Type);
                    this.PushToken(p);
                }
                else if (value is ValueToken v)
                {
                    v.ValueType = node.Type;
                    this.PushToken(value);
                }
                else
                {
                    this.PushToken(value);
                }
            }

            if (node.NodeType != ExpressionType.Not)
                return expr;
            if (this.OnTranslateExpression(node, node.Type))
                return expr;

            LTSQLToken token = this.PopToken();
            if (token is INotable notable)
                token = notable.Not();
            else
                throw new Exception($"表达式不支持取反操作：{node}");

            this.PushToken(token);
            return expr;
        }
        //条件表达式：三元运算符
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            Expression expr = base.VisitConditional(node);
            if (this.OnTranslateExpression(node, node.Type))
                return expr;
            
            LTSQLToken thenElse = this.PopToken(); // else 的值
            LTSQLToken thenValue = this.PopToken(); // then 的值
            LTSQLToken then = this.PopToken(); // then 的判断

            this.PushToken(LTSQLTokenFactory.CreatePriorityCalcToken(
                    LTSQLTokenFactory.CreateSwitchCase(then, thenValue, thenElse, node.Type)
                ));
            return expr;
        }


        public LTSQLToken Translate(QueryPart query, LTSQLOptions options)
        {
            return this.Translate(query, new LTSQLTranslateScope(LTSQLContext.Create(options)));
        }
        public LTSQLToken Translate(QueryPart query, LTSQLTranslateScope scope)
        {
            if (query as SqlQueryPart == null)
                throw new Exception($"不支持的查询类型：{query.GetType().Name}; 当前翻译器{nameof(SequenceTranslater)}只支持查询类型{nameof(SqlQueryPart)}");

            scope.Context.Options.GetTableName ??= GetTableName;
            scope.Context.Options.GetColumnName ??= GetColumnName;

            this._scope = scope;
            this._context = scope.Context;
            this._context.Root = query as SqlQueryPart;
            this._tokens = new Stack<LTSQLToken>();
            this._bufferLayer = new Stack<(Expression expr, LTSQLToken token)>();

            return this.TranslateCore();
        }
    }
}
