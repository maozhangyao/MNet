using MNet.LTSQL.SqlQueryStructs;
using MNet.LTSQL.SqlTokens;
using MNet.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace MNet.LTSQL
{
    public static class LTSQLQueryableExtensions
    {
        private static void AddOrder(ref SqlQueryPart sequence, Expression expr, bool desc)
        {
            sequence = sequence.SetNextStep(QueryStepSeq.OrderBy);
            sequence.Orders ??= new List<OrderKeyPart>();
            sequence.Orders.Add(new OrderKeyPart() { Key = expr, Asc = !desc });
        }
        private static SqlQueryPart SetNextStep(this SqlQueryPart query, QueryStepSeq step, bool equals = true)
        {
            if (query.Step >= step)
            {
                if (equals && query.Step == step)
                    return query.CopyNew() as SqlQueryPart;

                return new SqlQueryPart()
                {
                    Step = step,
                    MappingType = query.MappingType,
                    From = query
                };
            }
            query.Step = step;
            return query.CopyNew() as SqlQueryPart;
        }


        /// <summary>
        /// 初始化查询对象，以支持LINQ语法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ILTSQLOrderedQueryable<T> AsLTSQL<T>(string tableName = null, string schema = null)
        {
            return AsLTSQL((T)default, tableName, schema);
        }
        public static ILTSQLOrderedQueryable<T> AsLTSQL<T>(this T obj)
        {
            return AsLTSQL<T>(obj, null);
        }
        /// <summary>
        /// 初始化查询对象，支持指定表名以及架构
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="tableName">表名</param>
        /// <param name="schema">表架构</param>
        /// <returns></returns>
        public static ILTSQLOrderedQueryable<T> AsLTSQL<T>(this T obj, string tableName, string schema = null)
        {
            TablePart tablePart = new TablePart(typeof(T));
            tablePart.Refer = obj;
            tablePart.Schema = schema;
            tablePart.TableName = tableName;

            SqlQueryPart query = new SqlQueryPart();
            query.Step = QueryStepSeq.From;
            query.MappingType = typeof(T);
            query.From = tablePart;

            var ltsql = new LTSQLObject<T>(query);
            return ltsql;
        }
        /// <summary>
        /// 初始化查询对象并指定from数据源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="frm"></param>
        /// <returns></returns>
        public static ILTSQLOrderedQueryable<T> AsLTSQL<T>(this ILTSQLObjectQueryable<T> frm)
        {
            SqlQueryPart query = new SqlQueryPart();
            query.Step = QueryStepSeq.From;
            query.MappingType = typeof(T);
            query.From = frm.Query.CopyNew();

            return new LTSQLObject<T>(query);
        }
        public static ILTSQLOrderedQueryable<T> AsLTSQL<T>(this ILTSQLObjectSetable<T> frm)
        {
            SqlQueryPart query = new SqlQueryPart();
            query.Step = QueryStepSeq.From;
            query.MappingType = typeof(T);
            query.From = frm.Query.CopyNew();

            return new LTSQLObject<T>(query);
        }
        /// <summary>
        /// 将序列直接转换为分组模式，即将整个数据序列当作一个分组区域，形如：
        ///         select count(*), max(cost), min(cost), sum(cost) from xxxx
        /// 不依赖任何分组关键，直接对整个数据源做聚合操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static ILTSQLObjectQueryable<IGrouping<T, T>> AsGroup<T>(this ILTSQLObjectQueryable<T> src)
        {
            src = src.AsLTSQL();
            SqlQueryPart query = src.SqlQuery.SetNextStep(QueryStepSeq.GroupBy);
            query.GroupFlag = true;
            //query.GroupKey = (Expression<Func<T, T>>)(p => p);
            query.GroupElement = (Expression<Func<T, T>>)(p => p);

            return new LTSQLObject<IGrouping<T, T>>(query);
        }


        /// <summary>
        /// 硬编码形式的select语句支持，如：
        ///     SELECT 'Mr. liu' as name, 18 as age, 'like books' as Description
        /// 
        /// 批量版本：将使用 union all 连接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ILTSQLOrderedQueryable<T> AsSelect<T>(this List<T> list)
        {
            if (list.IsEmpty())
                throw new ArgumentNullException(nameof(list));

            return AsSelect(list.ToArray());
        }
        public static ILTSQLOrderedQueryable<T> AsSelect<T>(this T[] list)
        {
            if (list.IsEmpty())
                throw new ArgumentNullException(nameof(list));

            ILTSQLOrderedQueryable<T> query = AsSelect(list[0]);
            if(list.Length > 1)
                query = query.AsSet(DbSetType.Union, false).AppendSet(list.Select(p => AsSelect(p)).ToArray()).AsLTSQL();
            return query;
        }
        public static ILTSQLOrderedQueryable<T> AsSelect<T>(this T obj)
        {
            Type t = typeof(T);
            if (t.IsPrimitive || obj is string)
            {
                //对于基元类型和string类型，无需解析字段，直接使用其值
                return AsSelect(() => obj);
            }

            ConstructorInfo[] cstrs = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (cstrs.IsEmpty())
                throw new Exception($"无法获取类型{t.Name}的构造函数");
            if (cstrs.Length > 1 && !cstrs.Any(x => x.GetParameters().Length == 0))
                throw new Exception($"类型{t.Name}的公共构造函数超过1个，无法确定使用哪个构造函数");

            ConstructorInfo construct = cstrs.FirstOrDefault(x => x.GetParameters().Length == 0) ?? cstrs[0];
            if (construct.GetParameters().Length <= 0)
            {
                //无参构造，设置成员初始化
                List<MemberBinding> binds = new List<MemberBinding>();
                FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public);
                foreach (FieldInfo field in fields)
                {
                    var value = field.GetValue(obj);
                    var bind = Expression.Bind(field, Expression.Constant(value, field.FieldType));
                    binds.Add(bind);
                }

                PropertyInfo[] props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.GetProperty);
                foreach (PropertyInfo prop in props)
                {
                    var value = prop.GetValue(obj);

                    var bind = Expression.Bind(prop, Expression.Constant(value, prop.PropertyType));
                    binds.Add(bind);
                }

                if (binds.Count <= 0)
                    throw new Exception($"未能获取类型{t.Name}的任何公共属性或者字段");

                NewExpression _new = Expression.New(construct);
                MemberInitExpression init = Expression.MemberInit(_new, binds.ToArray());
                Expression<Func<T>> expr = Expression.Lambda<Func<T>>(init);
                return AsSelect(expr);
            }
            else
            {
                //带参构造，设置参数初始化成员(匿名对象使用)
                List<MemberInfo> members = new List<MemberInfo>();
                List<Expression> memberValues = new List<Expression>();
                foreach (ParameterInfo p in construct.GetParameters())
                {
                    MemberInfo member = t.GetMember(p.Name)[0];
                    object value = null;
                    if (member is PropertyInfo prop)
                    {
                        value = prop.GetValue(obj);
                    }
                    else if (member is FieldInfo field)
                    {
                        value = field.GetValue(obj);
                    }
                    else
                    {
                        throw new Exception($"无法获取类型{t.Name}的成员{p.Name}");
                    }
                    members.Add(member);
                    memberValues.Add(Expression.Constant(value));
                }

                NewExpression _new = Expression.New(construct, memberValues.ToArray(), members.ToArray());
                Expression<Func<T>> expr = Expression.Lambda<Func<T>>(_new);
                return AsSelect(expr);
            }
        }
        public static ILTSQLOrderedQueryable<TResult> AsSelect<T, TResult>(this T obj, Expression<Func<T, TResult>> expr)
        {
            ParameterExpression parameter = expr.Parameters[0];
            ConstantExpression constant = Expression.Constant(obj, typeof(T));
            Expression newBody = new ExpressionModifier()
                                    .WithModifer(ExpressionType.Parameter, _ => constant)
                                    .ModifyParameter(expr, 0, true).Body;

            return Expression.Lambda<Func<TResult>>(newBody).AsSelect();
        }
        public static ILTSQLOrderedQueryable<TResult> AsSelect<T, TResult>(this T obj, Func<T, Expression<Func<TResult>>> getNewExpr)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (getNewExpr == null)
                throw new ArgumentNullException(nameof(getNewExpr));

            var expr = getNewExpr(obj);
            return AsSelect(expr);
        }
        public static ILTSQLOrderedQueryable<TResult> AsSelect<TResult>(this Expression<Func<TResult>> expr)
        {
            if (expr == null)
                throw new ArgumentNullException(nameof(expr));

            SqlQueryPart query = new SqlQueryPart();
            query.Step = QueryStepSeq.End;
            query.MappingType = typeof(TResult);
            query.SelectKey = Expression.Lambda<Func<TResult, TResult>>(expr.Body, Expression.Parameter(typeof(TResult)));

            return new LTSQLObject<TResult>(query);
        }


        /// <summary>
        /// 联表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static ILTSQLOrderedQueryable<T> AsJoin<T>(this ILTSQLObjectQueryable<T> src, JoinType flag)
        {
            return new LTSQLObject<T>(src.SqlQuery.CopyNew() as SqlQueryPart) { JoinFlag = flag };
        }
        /// <summary>
        /// 设置联接类型为左连接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static ILTSQLOrderedQueryable<T> AsLeft<T>(this ILTSQLObjectQueryable<T> src)
        {
            return src.AsJoin(JoinType.LeftJoin);
        }
        /// <summary>
        /// 设置联接类型为右联接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static ILTSQLOrderedQueryable<T> AsRight<T>(this ILTSQLObjectQueryable<T> src)
        {
            return src.AsJoin(JoinType.RightJoin);
        }
        public static ILTSQLOrderedQueryable<T> AsInner<T>(this ILTSQLObjectQueryable<T> src)
        {
            return src.AsJoin(JoinType.InnerJoin);
        }


        /// <summary>
        /// 集合化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="setType"></param>
        /// <param name="distinct">是否去重</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ILTSQLObjectSetable<T> AsSet<T>(this ILTSQLObjectQueryable<T> src, DbSetType setType, bool distinct = false)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            QuerySetPart set = new QuerySetPart(typeof(T), new QueryPart[] { src.Query.CopyNew() }, setType, distinct);
            return new LTSQLObject<T>(set);
        }
        public static ILTSQLObjectSetable<T> AsSet<T>(this ILTSQLObjectSetable<T> src, DbSetType setType, bool distinct = false)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            QuerySetPart set = new QuerySetPart(typeof(T), new QueryPart[] { src.Query.CopyNew() }, setType, distinct);
            return new LTSQLObject<T>(set);
        }
        /// <summary>
        /// 多集合共同取并集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="other"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public static ILTSQLObjectSetable<T> UnionSet<T>(this ILTSQLObjectQueryable<T> src, ILTSQLQueryable other, bool distinct = false)
        {
            return AsSet(src, DbSetType.Union, distinct).AppendSet(other);
        }
        public static ILTSQLObjectSetable<T> UnionSet<T>(this ILTSQLObjectSetable<T> src, ILTSQLQueryable other, bool distinct = false)
        {
            return AsSet(src, DbSetType.Union, distinct).AppendSet(other);
        }
        /// <summary>
        /// 多集合共同取交集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="other"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public static ILTSQLObjectSetable<T> IntersectSet<T>(this ILTSQLObjectQueryable<T> src, ILTSQLQueryable other, bool distinct = true)
        {
            return AsSet(src, DbSetType.Intersect, distinct).AppendSet(other);
        }
        public static ILTSQLObjectSetable<T> IntersectSet<T>(this ILTSQLObjectSetable<T> src, ILTSQLQueryable other, bool distinct = true)
        {
            return AsSet(src, DbSetType.Intersect, distinct).AppendSet(other);
        }
        /// <summary>
        /// 多集合共同取差集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="other"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public static ILTSQLObjectSetable<T> ExceptSet<T>(this ILTSQLObjectQueryable<T> src, ILTSQLQueryable other, bool distinct = true)
        {
            return AsSet(src, DbSetType.Except, distinct).AppendSet(other);
        }
        public static ILTSQLObjectSetable<T> ExceptSet<T>(this ILTSQLObjectSetable<T> src, ILTSQLQueryable other, bool distinct = true)
        {
            return AsSet(src, DbSetType.Except, distinct).AppendSet(other);
        }
        /// <summary>
        /// 向当前集合追加相同集合操作, 比如：
        /// 向并集集合，在追加集合做并集
        /// 向交集集合，在追加集合做并集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ILTSQLObjectSetable<T> AppendSet<T>(this ILTSQLObjectSetable<T> src, params ILTSQLQueryable[] other)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));
            if (other == null || other.Length <= 0)
                throw new ArgumentNullException(nameof(other));

            List<QueryPart> querys = new List<QueryPart>();
            querys.AddRange(src.SetQuery.Querys.Select(p => p.CopyNew()));
            querys.AddRange(other.Select(p => p.Query.CopyNew()));

            QuerySetPart set = new QuerySetPart(typeof(T), querys, src.SetQuery.SetType, src.SetQuery.Distinct);
            return new LTSQLObject<T>(set);
        }


        /// <summary>
        /// 分页：跳过指定元素个数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        public static ILTSQLObjectQueryable<T> Skip<T>(this ILTSQLObjectQueryable<T> src, int skip)
        {
            SqlQueryPart query = src.SqlQuery.SetNextStep(QueryStepSeq.Page);
            query.Skip = skip;
            //主要是，数据库中没有独立使用Skip的场景，所以默认设置一个最大的Take值代替
            if (query.Take == null)
                query.Take = int.MaxValue;

            return new LTSQLObject<T>(query);
        }
        /// <summary>
        ///  分页：仅取指定个数元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public static ILTSQLObjectQueryable<T> Take<T>(this ILTSQLObjectQueryable<T> src, int take)
        {
            SqlQueryPart query = src.SqlQuery.SetNextStep(QueryStepSeq.Page);
            query.Take = take;
            return new LTSQLObject<T>(query);
        }
        /// <summary>
        /// 去重
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static ILTSQLObjectQueryable<T> Distinct<T>(this ILTSQLObjectQueryable<T> src)
        {
            SqlQueryPart query = src.SqlQuery.SetNextStep(QueryStepSeq.Query);
            query.Distinct = true;

            return new LTSQLObject<T>(query);
        }
        //where
        public static ILTSQLObjectQueryable<T> Where<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, bool>> expr)
        {
            SqlQueryPart query = src.SqlQuery.SetNextStep(QueryStepSeq.Where);
            query.Where = query.Where == null ? expr : expr.MergeAnd(query.Where as Expression<Func<T, bool>>);
            return new LTSQLObject<T>(query);
        }
        //having
        public static ILTSQLObjectQueryable<IGrouping<TKey, T>> Where<T, TKey>(this ILTSQLObjectQueryable<IGrouping<TKey, T>> src, Expression<Func<IGrouping<TKey, T>, bool>> expr)
        {
            SqlQueryPart query = src.SqlQuery.SetNextStep(QueryStepSeq.Having);
            query.Having = query.Having == null ? expr : expr.MergeAnd(query.Having as Expression<Func<IGrouping<TKey, T>, bool>>);
            return new LTSQLObject<IGrouping<TKey, T>>(query);
        }
        //order
        public static ILTSQLOrderedQueryable<T> OrderBy<T, TKey>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.SqlQuery.CopyNew() as SqlQueryPart;
            AddOrder(ref query, keyExpr, false);

            return new LTSQLObject<T>(query);
        }
        public static ILTSQLOrderedQueryable<T> OrderByDescending<T, TKey>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.SqlQuery.CopyNew() as SqlQueryPart;
            AddOrder(ref query, keyExpr, true);

            return new LTSQLObject<T>(query);
        }
        public static ILTSQLOrderedQueryable<T> ThenBy<T, TKey>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.SqlQuery.CopyNew() as SqlQueryPart;
            AddOrder(ref query, keyExpr, false);
            ;
            return new LTSQLObject<T>(query);
        }
        public static ILTSQLOrderedQueryable<T> ThenByDescending<T, TKey>(this ILTSQLOrderedQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            var query = src.SqlQuery.CopyNew() as SqlQueryPart;
            AddOrder(ref query, keyExpr, true);

            return new LTSQLObject<T>(query);
        }
        //group
        public static ILTSQLObjectQueryable<IGrouping<TKey, T>> GroupBy<T, TKey>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr)
        {
            //默认的分组元素为整个对象
            return GroupBy(src, keyExpr, p => p);
        }
        public static ILTSQLObjectQueryable<IGrouping<TKey, TElement>> GroupBy<T, TKey, TElement>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TKey>> keyExpr, Expression<Func<T, TElement>> elementExpr)
        {
            var query = src.SqlQuery.SetNextStep(QueryStepSeq.GroupBy, false);

            query.GroupFlag = true;
            query.GroupKey = keyExpr;
            query.GroupElement = elementExpr;

            return new LTSQLObject<IGrouping<TKey, TElement>>(query);
        }
        //join
        public static ILTSQLObjectQueryable<TResult> Join<TOuter, TInner, TKey, TResult>(this ILTSQLObjectQueryable<TOuter> outer
            , ILTSQLObjectQueryable<TInner> inner
            , Expression<Func<TOuter, TKey>> outerKeyExpr
            , Expression<Func<TInner, TKey>> innerKeyExpr
            , Expression<Func<TOuter, TInner, TResult>> joinExpr)
        {
            SqlQueryPart qOuter = outer.SqlQuery.CopyNew() as SqlQueryPart;
            SqlQueryPart qInner = inner.SqlQuery.CopyNew() as SqlQueryPart;
            JoinPart joinPart = new JoinPart();

            //如果是手工方法调用，则需要检验join表达式中，参数命名是否能够推出表命名来
            joinPart.JoinType = (inner as LTSQLObject<TInner>).JoinFlag;
            joinPart.JoinKey1 = outerKeyExpr;
            joinPart.JoinKey2 = innerKeyExpr;
            joinPart.JoinObject = joinExpr;
            joinPart.MappingType = typeof(TResult);

            //直接作为子查询
            joinPart.MainQuery = qOuter;
            if ((int)qOuter.Step <= (int)QueryStepSeq.Join)
                //连续的join
                joinPart.MainQuery = qOuter.From;

            //join 子查询
            joinPart.JoinQuery = qInner;
            if ((int)qInner.Step < (int)QueryStepSeq.Join)
                //join 一张表
                joinPart.JoinQuery = qInner.From;


            return new LTSQLObject<TResult>(new SqlQueryPart
            {
                From = joinPart,
                Step = QueryStepSeq.Join,
                MappingType = typeof(TResult)
            });
        }

        // select Many : 注意：内部会调用TSource 和 TCollectioin 类型的构造函数，请确保构造函数无特定逻辑
        public static ILTSQLObjectQueryable<TResult> SelectMany<TSource, TCollection, TResult>(this ILTSQLObjectQueryable<TSource> source
            , Expression<Func<TSource, ILTSQLObjectQueryable<TCollection>>> collectionSelector
            , Expression<Func<TSource, TCollection, TResult>> resultSelector)
        {
            ILTSQLObjectQueryable<TCollection> inner = collectionSelector.Compile().Invoke(default(TSource));

            SqlQueryPart qOuter = source.SqlQuery.CopyNew() as SqlQueryPart;
            SqlQueryPart qInner = inner.SqlQuery.CopyNew() as SqlQueryPart;

            JoinPart join = new JoinPart();
            join.MappingType = typeof(TResult);
            join.JoinObject = resultSelector;
            join.MainQuery = qOuter.From;
            //非连续join
            if (qOuter.Step > QueryStepSeq.Join)
                join.MainQuery = qOuter;

            join.JoinQuery = qInner;
            if (qInner.Step < QueryStepSeq.Join)
                join.JoinQuery = qInner.From;

            SqlQueryPart query = new SqlQueryPart();
            query.From = join;
            query.Step = QueryStepSeq.Join;
            query.MappingType = typeof(TResult);
            return new LTSQLObject<TResult>(query);
        }

        //不支持 GroupJoin
        public static ILTSQLObjectQueryable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this ILTSQLObjectQueryable<TOuter> outer
            , ILTSQLObjectQueryable<TInner> inner
            , Expression<Func<TOuter, TKey>> outerKeySelector
            , Expression<Func<TInner, TKey>> innerKeySelector
            , Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector)
        {
            throw new Exception("不支持Join into 写法，请使用Join代替。");
        }
        //select
        public static ILTSQLObjectQueryable<TResult> Select<T, TResult>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> expr)
        {
            //Console.WriteLine(expr);
            Expression selectKeyExpr = expr;
            SqlQueryPart _old = src.Query as SqlQueryPart;
            if (_old.Step == QueryStepSeq.Select)
            {
                LambdaExpression lambda = _old.SelectKey.AsLambda();
                if (lambda == null)
                    throw new Exception($"在连续select过程中，未能取得上一次select的表达式(当前select:{expr})。");
                if (lambda.ReturnType != typeof(T))
                    throw new Exception($"在连续select过程中，上一次select返回值类型({lambda.ReturnType.FullName})与当前select入参类型不匹配({typeof(T).FullName})。");

                //ExpressionModifier modifier = new ExpressionModifier();
                Expression _oldPara = lambda.TakeParamter(0);
                Expression _newbody = new ExpressionModifier()
                    .WithModifer(ExpressionType.Parameter, _ => lambda.Body)
                    .ModifyParameter(expr.Body, expr.TakeParamter(0));

                Expression _newExpr = Expression.Lambda(_newbody, _oldPara as ParameterExpression);
                selectKeyExpr = _newExpr;
            }

            SqlQueryPart _new = (src.SqlQuery.CopyNew() as SqlQueryPart)
               .SetNextStep(QueryStepSeq.Select, true); //连续的select只需要取最后一次

            _new.SelectKey = selectKeyExpr;
            _new.MappingType = typeof(TResult);
            return new LTSQLObject<TResult>(_new);
        }

        /// <summary>
        /// 相当于 sql 的 exsits 函数(终结点函数)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static ILTSQLObjectQueryable<bool> ToAny<T>(this ILTSQLObjectQueryable<T> src)
        {
            src = new LTSQLObject<T>(src.SqlQuery.CopyNew() as SqlQueryPart);
            return AsSelect(() => src.Any());
        }
        /// <summary>
        /// 终结点聚合函数，形如：
        ///     select  count(*) from xxxx
        ///     select sum(cost) from xxxx
        /// 整个查询仅返回单个聚合值。当查询执行了终结点聚合函数，表示整个查询的最终结果了。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="src"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static ILTSQLObjectQueryable<TResult> ToSum<T, TResult>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> selector)
        {
            return InternalExpressionGenerator.Sum(src, selector);
        }
        public static ILTSQLObjectQueryable<TResult> ToMax<T, TResult>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> selector)
        {
            return InternalExpressionGenerator.Max(src, selector);
        }
        public static ILTSQLObjectQueryable<TResult> ToMin<T, TResult>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> selector)
        {
            return InternalExpressionGenerator.Min(src, selector);
        }
        public static ILTSQLObjectQueryable<TResult> ToAverage<T, TResult>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> selector)
        {
            return InternalExpressionGenerator.Average(src, selector);
        }
        public static ILTSQLObjectQueryable<int> ToCount<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, object>> selector = null)
        {
            return InternalExpressionGenerator.Count(src, selector);
        }
        public static ILTSQLObjectQueryable<long> ToLongCount<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, object>> selector = null)
        {
            return InternalExpressionGenerator.LongCount(src, selector);
        }


        public static ILTSQLNonQueryable<T> AsUpdate<T>(Expression<Func<T, object>> setUpdate)
        {
            if (setUpdate == null)
                throw new ArgumentNullException(nameof(setUpdate));

            return AsUpdate(null, null, setUpdate);
        }
        public static ILTSQLNonQueryable<T> AsUpdate<T>(string table, string schema, Expression<Func<T, object>> setUpdate)
        {
            return AsUpdate<T>(default, setUpdate, table, schema);
        }
        public static ILTSQLNonQueryable<T> AsUpdate<T>(this T entity, Expression<Func<T, object>> setUpdate, string table = null, string schema = null)
        {
            if (setUpdate == null)
                throw new ArgumentNullException(nameof(setUpdate));

            return new LTSQLObject<T>(new UpdatePart()
            {
                Refer = entity,
                Schema = schema,
                TableName = table,
                MappingType = typeof(T)
            }).SetUpdate(setUpdate);
        }
        public static ILTSQLNonQueryable<T> SetUpdate<T>(this ILTSQLNonQueryable<T> nonQuery, Expression<Func<T, object>> setUpdate)
        {
            if (setUpdate == null)
                throw new ArgumentNullException(nameof(setUpdate));

            UpdatePart part = nonQuery.Query as UpdatePart;
            if (part == null)
                throw new Exception($"非法的{nameof(QueryPart)}");

            part = part.CopyNew() as UpdatePart;
            part.SetUpdate = setUpdate;
            return new LTSQLObject<T>(part);
        }

        public static ILTSQLNonQueryable<T> AsDelete<T>()
        {
            return AsDelete<T>(null);
        }
        public static ILTSQLNonQueryable<T> AsDelete<T>(Expression<Func<T, bool>> expr)
        {
            return AsDelete<T>(null, null, expr);
        }
        public static ILTSQLNonQueryable<T> AsDelete<T>(string table, string schema, Expression<Func<T, bool>> expr)
        {
            return AsDelete(default, expr, table, schema);
        }
        public static ILTSQLNonQueryable<T> AsDelete<T>(this T entity, Expression<Func<T, bool>> expr = null, string table = null, string schema = null)
        {
            LTSQLObject<T> obj = new LTSQLObject<T>(new DeletePart()
            {
                Refer = entity,
                Schema = schema,
                TableName = table,
                MappingType = typeof(T),
            });

            return Where((ILTSQLNonQueryable<T>)obj, expr);
        }
        public static ILTSQLNonQueryable<T> Where<T>(this ILTSQLNonQueryable<T> nonQuery, Expression<Func<T, bool>> expr)
        {
            NonQueryPart part = nonQuery.Query as NonQueryPart;
            if (part == null)
                throw new Exception($"非法的{nameof(QueryPart)}");
            if (expr == null)
                throw new ArgumentNullException(nameof(expr));

            part = part.CopyNew() as NonQueryPart;
            if (part.Where == null)
            {
                part.Where = expr;
                return new LTSQLObject<T>(part);
            }

            part.Where = ExpressionUtils.MergeAnd((Expression<Func<T, bool>>)part.Where, expr);
            return new LTSQLObject<T>(part);
        }


        #region sql格式化
        /// <summary>
        /// 返回非参数化的sql(使用LTSQLOptionsSetting配置类作为默认配置)
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string ToSql(this ILTSQLQueryable src)
        {
            LTSQLOptions opt = LTSQLOptionsSetting.GetOptions();
            if (opt == null)
                throw new Exception($"请指定{nameof(LTSQLOptions)}配置，可以考虑设置{nameof(LTSQLOptionsSetting)}配置类.");

            opt.UseSqlParameter = false;
            return ToSql(src, out _, opt, null);
        }
        /// <summary>
        /// 返回参数化的sql(使用LTSQLOptionsSetting配置类作为默认配置)
        /// </summary>
        /// <param name="src"></param>
        /// <param name="sqlargs"></param>
        /// <returns></returns>
        public static string ToSql(this ILTSQLQueryable src, out List<(string key, object val)> sqlargs)
        {
            LTSQLOptions opt = LTSQLOptionsSetting.GetOptions();
            if (opt == null)
                throw new Exception($"请指定{nameof(LTSQLOptions)}配置，可以考虑设置{nameof(LTSQLOptionsSetting)}配置类.");
            
            opt.UseSqlParameter = true;
            return ToSql(src, out sqlargs, opt, null);
        }
        /// <summary>
        /// 生成指定数据库的sql，并返回非参数化sql
        /// </summary>
        /// <param name="src"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static string ToSql(this ILTSQLQueryable src, DbTypes db)
        {
            return ToSql(src, db, out _, false);
        }
        /// <summary>
        ///  生成指定数据库的sql，并返回参数化sql
        /// </summary>
        /// <param name="src"></param>
        /// <param name="db"></param>
        /// <param name="sqlargs"></param>
        /// <returns></returns>
        public static string ToSql(this ILTSQLQueryable src, DbTypes db, out List<(string key, object val)> sqlargs)
        {
            return ToSql(src, db, out sqlargs, true);
        }
        public static (string, List<(string key, object val)>) ToSqlWithParameter(this ILTSQLQueryable src, DbTypes db, bool useSqlParameter = true)
        {
            LTSQLOptions opt = LTSQLOptionsSetting.GetOptions() ?? new LTSQLOptions();
            opt.DbType = db;
            opt.UseSqlParameter = useSqlParameter;

            return ToSqlWithParameter(src, opt, null);
        }
        public static string ToSql(this ILTSQLQueryable src, DbTypes db, out List<(string key, object val)> sqlargs, bool useSqlParameter = true)
        {
            LTSQLOptions opt = LTSQLOptionsSetting.GetOptions() ?? new LTSQLOptions();
            opt.DbType = db;
            opt.UseSqlParameter = useSqlParameter;

            return ToSql(src, out sqlargs, opt, null);
        }

        public static (string, List<(string key, object val)>) ToSqlWithParameter(this ILTSQLQueryable src, LTSQLOptions options = null, SqlBuilderOptions ctx = null)
        {
            List<(string key, object val)> list = null;
            string sql = ToSql(src, out list, options, ctx);
            return (sql, list);
        }
        public static string ToSql(this ILTSQLQueryable src, out List<(string key, object val)> sqlargs, LTSQLOptions options = null, SqlBuilderOptions ctx = null)
        {
            if (src == null || src.Query == null)
                throw new Exception("参数或者Query对象实例为null.");

            options ??= LTSQLOptionsSetting.GetOptions() ?? throw new Exception($"请指定{nameof(LTSQLOptions)}配置，可以考虑设置{nameof(LTSQLOptionsSetting)}配置类.");

            QueryPart q = src.Query.CopyNew();
            IQueryTranslaterFactory factory = new QueryTranslaterFactory();
            IQueryTranslater tranlator = factory.Create(q);
            if (tranlator == null)
                throw new Exception($"未配置{q.GetType().FullName}类型的SQL翻译器");

            LTSQLToken token = tranlator.Translate(q, new LTSQLTranslateScope(LTSQLContext.Create(options))).BreakClause();
            SqlBuilderOptions bCtx = ctx ?? LTSQLOptionsSetting.GetSqlBuildOptions(options);
            ISqlBuilder builder = LTSQLTokenSqlBuilder.Default;

            if (options.ConfigSqlBuilderOptions != null)
                options.ConfigSqlBuilderOptions(options, bCtx);

            builder.Build(token, bCtx);
            sqlargs = bCtx.SqlParameters;
            return bCtx.Sql.ToString();
        }
        #endregion
    }
}
