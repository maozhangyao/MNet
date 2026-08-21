using MNet.LTSQL.Attributes;
using MNet.LTSQL.Objects;
using MNet.LTSQL.SqlQueryStructs;
using MNet.LTSQL.SqlTokenExtends;
using MNet.LTSQL.SqlTokens;
using MNet.LTSQL.TypeModels;
using MNet.Utils;
using System;
using System.Collections.Generic;
using System.Data;

#if NET6_0_OR_GREATER
using System.ComponentModel.DataAnnotations.Schema;
#endif
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;

namespace MNet.LTSQL
{    
    //开启翻译
    public class SequenceTranslater : ExpressionTranslater, IQueryTranslater
    {
        public SequenceTranslater()
        { }


        private string _transparentField = "transparent_field";


        //删除所有的表格token，将其转换为元组,并且调整为新的所属者
        private TupleToken ChangePropOwner(ITupleable tuple, ObjectToken obj)
        {
            if (tuple == null)
                return null;

            TupleToken _new = new TupleToken(tuple.MappingType);
            foreach ((string key, LTSQLToken val) in tuple)
            {
                LTSQLToken newVal = null;
                if (val is ITupleable sub)
                {
                    newVal = this.ChangePropOwner(sub, obj);
                }
                else
                {
                    newVal = LTSQLTokenFactory.CreateAccessToken(obj, key, tuple.GetValueType(key));
                }

                _new.Add(key, newVal, tuple.GetValueType(key));
            }

            return _new;
        }
        private LTSQLToken TranslateFrom(QueryPart from, string root, out TupleToken fieldToken)
        {
            LTSQLToken token = TranslateQueryPart(from, root, out fieldToken);
            return token;
        }
        private LTSQLToken TranslateQueryPart(QueryPart from, string parameterName, out TupleToken fieldToken)
        {
            string tableAlias = null;

            LTSQLToken src = null;
            TableRefToken tbRef = null;
            EntityTypeDescriptor descriptor = null;

            fieldToken = null;

            if (from is JoinPart join)
            {
                LTSQLToken query1 = this.TranslateQueryPart(join.MainQuery, join.JoinObject.AsLambda().TakeParamter(0).Name, out _);
                LTSQLToken query2 = this.TranslateQueryPart(join.JoinQuery, join.JoinObject.AsLambda().TakeParamter(1).Name, out _);

                //解析透明表结构
                LTSQLToken tbMerge = this.TranslateLambda(join.JoinObject.AsLambda());
                if (tbMerge is ITupleable tbMergeObj)
                {
                    ITupleable expd = tbMergeObj.ExpendTuple(join.MappingType);
                    fieldToken = LTSQLTokenFactory.CreateTupleToken(expd);
                }
                else
                {
                    //理论上不存在
                    fieldToken = LTSQLTokenFactory.CreateTupleToken(join.MappingType);
                    fieldToken.Add(_transparentField, tbMerge, ((ValueToken)tbMerge).ValueType);
                }

                // Join 过程中的Join object 与 select 返回的 object 是一个意思，都是tuple，而不是 table ref。
                // 因为 join object 中的属性与select 返回的 object 的属性是一样的，都是计算属性(不同表格的字段参与的运算)。
                // 示例：
                //      from a in obj1
                //      join b in obj2
                //      select new { aId = a.Id, bId = b.Id, FullId = a.Id + b.Id }
                // 如上述所示，join object 和 select object 共用的，编译器不会再调用select函数。
                //this.Context.SetScopeParameter(parameterName, LTSQLTokenFactory.CreateTupleToken(descriptor));
                this.Context.ParameterMgr.InjectParameter(parameterName, tbMerge);

                //连接查询
                //合并查询
                if (join.JoinKey1 != null)
                {
                    LambdaExpression expr1 = join.JoinKey1.AsLambda();
                    LambdaExpression expr2 = join.JoinKey2.AsLambda();
                    LambdaExpression expr3 = Expression.Lambda(Expression.Equal(expr1.Body, expr2.Body), expr1.TakeParamter(0), expr2.TakeParamter(0));
                    LTSQLToken joinKeys = this.TranslateLambda(expr3);
                    JoinToken joinToken = LTSQLTokenFactory.CreateJoinToken(join.JoinType, query1, query2, joinKeys);
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
                tableAlias = this.Context.TableAliasGenerator.Next();
                descriptor = this.GetEntityTypeDescriptor(from.MappingType, table.Schema, table.TableName, table.Refer);
                src = LTSQLTokenFactory.CreateTableObjectToken(descriptor.TableName, descriptor, table.MappingType);
                tbRef = LTSQLTokenFactory.CreateTableRefToken(tableAlias, descriptor);
                fieldToken = ChangePropOwner(tbRef, tbRef);

                this.Context.ParameterMgr.InjectParameter(parameterName, tbRef);
            }
            else
            {
                IQueryTranslater translater = new QueryTranslaterFactory().Create(from);
                if (translater == null)
                    throw new Exception($"不支持的查询结构:{from.GetType().FullName}");

                LTSQLToken qry = src = translater.Translate(from, this.Scope.NewScope())
                    .TryPriority(true);

                //解析字段
                if (qry is ISelectable select)
                {
                    tableAlias = this.Context.TableAliasGenerator.Next();
                    ObjectToken objectRef = LTSQLTokenFactory.CreateObjectToken(SqlObjectType.RefObject, tableAlias, from.MappingType);

                    // Linq 中属性可以存储复杂对象，通过select可以将复杂对象一直向上传递引用，使得上层linq 作用域能够访问底层的复杂对象属性。
                    // 但是SQL中不可能支持字段表示复杂对象，所以当翻译存储复杂对象的属性时，需要将该复杂对象表示为tuple，将属性访问的值变成tuple值。
                    TupleToken newRef = ChangePropOwner(select, objectRef);

                    fieldToken = LTSQLTokenFactory.CreateTupleToken(newRef.ExpendTuple(from.MappingType));
                    this.Context.ParameterMgr.InjectParameter(parameterName, newRef);
                }
                src = qry;
            }

            return LTSQLTokenFactory.CreateAliasToken(src, tableAlias);
        }
        private LTSQLToken TranslateWhere(LambdaExpression wheres)
        {
            if (wheres == null)
                return null;

            LTSQLToken token = this.TranslateLambda(wheres);

            return token;
        }
        private LTSQLToken[] TranslateGroup(LambdaExpression groupKey, LambdaExpression groupEle, out GroupObjToken groupToken)
        {
            groupToken = null;
            LTSQLToken groupKeyToken = null;
            LTSQLToken groupEleToken = null;
            List<LTSQLToken> groupKeyTokens = new List<LTSQLToken>();

            //分组元素翻译
            if (groupEle != null)
                groupEleToken = this.TranslateLambda(groupEle);

            //分组依据翻译
            if (groupKey != null)
            {
                groupKeyToken = this.TranslateLambda(groupKey);

                if (groupKeyToken is TupleToken tuple)
                    groupKeyTokens.AddRange(tuple.PropValues.ToArray());
                else
                    groupKeyTokens.Add(groupKeyToken);
            }

            groupToken = new GroupObjToken(groupEleToken, groupKeyToken);
            if (groupKeyTokens.Count <= 0)
                return null; //不是直接group by操作，可能是直接做单一的聚合查询，如 select count(1) from xxx

            return groupKeyTokens.ToArray();
        }
        private LTSQLToken TranslateHaving(LambdaExpression havings)
        {
            if (havings == null)
                return null;

            try
            {
                LTSQLToken token = this.TranslateLambda(havings);
                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
        private LTSQLToken[] TranslateOrder(List<OrderKeyPart> orders)
        {
            if (orders.IsEmpty())
                return null;

            List<LTSQLToken> orderKeyTokens = new List<LTSQLToken>();
            foreach (OrderKeyPart getKey in orders)
            {
                LambdaExpression lambda = getKey.Key.AsLambda();
                LTSQLToken token = this.TranslateLambda(lambda);
                orderKeyTokens.Add(LTSQLTokenFactory.CreateOrderByItemToken(token, !getKey.Asc));
            }

            return orderKeyTokens.ToArray();
        }
        private LTSQLToken[] TranslateSelect(LambdaExpression selectKey, out TupleToken selectTuple)
        {
            selectTuple = new TupleToken(selectKey.Type);
            try
            {
                LTSQLToken token = this.TranslateLambda(selectKey);
                List<LTSQLToken> fields = new List<LTSQLToken>();

                if (token is ITupleable tuple)
                {
                    //对于select，需要展开元组(需要解决key冲突问题，保持唯一性)
                    ITupleable expdTuple = tuple.ExpendTuple(selectKey.ReturnType);
                    fields.AddRange(expdTuple.Select(p => LTSQLTokenFactory.CreateAliasToken(p.Item2, p.Item1)));

                    //对于子tuple需要保持原样，用于上层查询访问，所以无需展开。因为select中没有命名范围，所以也无法为tuple指定对象名。
                    foreach ((string key, LTSQLToken val) in tuple)
                        selectTuple.Add(key, val, tuple.GetValueType(key));
                }
                else if (token is AccessPropertyToken access)
                {
                    fields.Add(token);
                    selectTuple.Add(access.Prop.OrginFieldName ?? access.Prop.FieldName, access, access.ValueType);
                }
                else
                {
                    fields.Add(LTSQLTokenFactory.CreateAliasToken(token, _transparentField));
                    selectTuple.Add(_transparentField, token, selectKey.ReturnType);
                }

                return fields.ToArray();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        
        //统一命名
        private void BeforeTranslate(SqlQueryPart query, ref string root)
        {
            if (query == null)
                return;

            root = "root_" + this.Context.ParameterNameGenerator.Next();
            ExpressionModifier exprModifier = new ExpressionModifier();

            //统一根参数名(存在select 字段硬编码查询)
            if (query.Where != null)
            {
                LambdaExpression lambda = query.Where.AsLambda();
                ParameterExpression _old = lambda.TakeParamter(0);
                ParameterExpression _new = Expression.Parameter(_old.Type, root);
                query.Where = exprModifier.WithParameterModifier(_ => _new)
                    .ModifyParameter(lambda, _old);
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
                    query.GroupKey = exprModifier
                        .WithParameterModifier(_ => _new)
                        .ModifyParameter(query.GroupKey, _old);
                }

                if (query.GroupElement != null)
                {
                    _old = query.GroupElement.AsLambda().TakeParamter(0);
                    _new = Expression.Parameter(_old.Type, root);
                    query.GroupElement = exprModifier
                        .WithParameterModifier(_ => _new)
                        .ModifyParameter(query.GroupElement, _old);
                }
            }

            // having
            if (query.Having != null)
            {
                LambdaExpression lambda = query.Having.AsLambda();
                ParameterExpression _old = lambda.AsLambda().TakeParamter(0);
                ParameterExpression _new = Expression.Parameter(_old.Type, root);
                query.Having = exprModifier.WithParameterModifier(_ => _new)
                    .ModifyParameter(lambda, _old);
            }

            //排序
            if (query.Orders.IsNotEmpty())
            {
                ParameterExpression _old = query.Orders[0].Key.AsLambda().TakeParamter(0);
                ParameterExpression _new = Expression.Parameter(_old.Type, root);
                exprModifier.WithParameterModifier(_ => _new);

                foreach (OrderKeyPart orderItem in query.Orders)
                {
                    LambdaExpression lambda = orderItem.Key.AsLambda();
                    orderItem.Key = exprModifier.ModifyParameter(lambda, lambda.TakeParamter(0));
                }
            }

            //投影
            if (query.SelectKey != null)
            {
                LambdaExpression lambda = query.SelectKey.AsLambda();
                ParameterExpression _old = lambda.TakeParamter(0);
                ParameterExpression _new = Expression.Parameter(_old.Type, root);
                query.SelectKey = exprModifier
                    .WithParameterModifier(_ => _new)
                    .ModifyParameter(lambda, _old);
            }

        }
        // query 翻译
        private SqlQueryToken TranslateQueryCore(SqlQueryPart query)
        {
            string root = null;
            LTSQLToken parameterObj = null;
            TupleToken selectTuple = null;
            SqlQueryToken sqlToken = null;

            LTSQLToken from = null;
            LTSQLToken where = null;
            LTSQLToken order = null;
            LTSQLToken group = null;
            LTSQLToken having = null;
            LTSQLToken page = null;
            LTSQLToken select = null;

            //
            this.BeforeTranslate(query, ref root);

            //from, 注意存在单独的select 语句：select 1
            //from 是可能null的
            if (query.From != null)
            {
                LTSQLToken src = this.TranslateFrom(query.From, root, out selectTuple);
                from = LTSQLTokenFactory.CreateFromClauseToken(src);
            }

            //where
            if (query.Where != null)
            {
                LTSQLToken condition = this.TranslateWhere(query.Where.AsLambda());
                where = LTSQLTokenFactory.CreateWhereClauseToken(condition);
            }

            //group by，注意存在select count(*) from xxx ；即无需group by 子句的全部数据分组
            if (query.GroupFlag)
            {
                LambdaExpression lambda1 = query.GroupKey.AsLambda();
                LambdaExpression lambda2 = query.GroupElement.AsLambda();

                LTSQLToken[] groupKeys = this.TranslateGroup(lambda1, lambda2, out GroupObjToken groupObj);
                if (groupKeys != null)
                    group = LTSQLTokenFactory.CreateGroupClauseToken(groupKeys);

                parameterObj = groupObj;
                this.Context.ParameterMgr.InjectParameter(root, parameterObj);
            }

            //having
            if (query.Having != null)
            {
                LTSQLToken condition = this.TranslateHaving(query.Having.AsLambda());
                having = LTSQLTokenFactory.CreateHavingClauseToken(condition);
            }

            //order by
            if (query.Orders.IsNotEmpty())
            {
                LTSQLToken[] orderKeys = this.TranslateOrder(query.Orders);
                order = LTSQLTokenFactory.CreateOrderByClauseToken(orderKeys);
            }

            //select
            LTSQLToken[] selectFields = null;
            if (query.SelectKey != null)
            {
                selectFields = this.TranslateSelect(query.SelectKey.AsLambda(), out selectTuple);
            }
            else
            {
                //需要注意字段唯一命名问题
                selectFields = selectTuple.Select(p => LTSQLTokenFactory.CreateAliasToken(p.value, p.key)).ToArray();
            }

            //distict
            LTSQLToken distinckClause = null;
            if (query.Distinct)
                distinckClause = LTSQLTokenFactory.CreateDistinctToken();

            //分页
            LTSQLToken topLimitClause = null;
            if ((query.Skip == null || query.Skip == 0) && query.Take != null && this.Context.Options?.DbType == DbTypes.MSSQL)
            {
                //sql server 的 top 语法
                SqlParameterToken sqlParam = LTSQLTokenFactory.CreateSqlParameterToken(this.Context.ParameterNameGenerator.Next(), query.Take, typeof(int));
                topLimitClause = LTSQLTokenFactory.CreateTopClauseToken(sqlParam);
            }
            else if (query.Take != null)
            {
                page = LTSQLTokenFactory.CreatePageToken(query.Skip ?? 0, query.Take);
            }

            select = LTSQLTokenFactory.CreateSelectClauseToken(selectFields, distinckClause, topLimitClause);
            sqlToken = LTSQLTokenFactory.CreateSqlQueryToken(selectTuple, from, where, group, having, order, page, select, false);
            sqlToken = sqlToken.ChangeType(typeof(ILTSQLObjectQueryable<>).MakeGenericType(query.MappingType)) as SqlQueryToken;
            sqlToken = this.PostTranslate(sqlToken) as SqlQueryToken;
            return sqlToken;
        }
        
        public LTSQLToken Translate(QueryPart query, LTSQLTranslateScope scope)
        {
            if (query as SqlQueryPart == null)
                throw new Exception($"不支持的查询类型：{query.GetType().Name}");

            this.ApplyScope(scope);

            this.Context.Part = query;
            try
            {
                this.Context.ParameterMgr.PushScope();
                return this.TranslateQueryCore(query as SqlQueryPart);
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                this.Context.ParameterMgr.PopScope();
            }
        }
    }
}
