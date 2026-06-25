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
        private static T Active<T>()
        {
            return (T)Active(typeof(T));
        }
        private static object Active(Type t)
        {
            var cstrs = t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            ConstructorInfo minParameters = null;
            foreach (var cstr in cstrs)
            {
                if (minParameters == null)
                    minParameters = cstr;
                if (cstr.GetParameters().Length < minParameters.GetParameters().Length)
                    minParameters = cstr;
            }

            ParameterInfo[] ps = minParameters.GetParameters();
            object[] objs = ps.Select(p =>
            {
                if (p.ParameterType.IsClass)
                    return (object)null;
                if (p.ParameterType.IsValueType && !p.ParameterType.IsEnum && !p.ParameterType.IsPrimitive)
                    return Activator.CreateInstance(p.ParameterType); //结构体，直接构造
                return (object)0;
            }).ToArray();

            return minParameters.Invoke(objs);
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
        private static void AddOrder(ref SqlQueryPart sequence, Expression expr, bool desc)
        {
            sequence = sequence.SetNextStep(QueryStepSeq.OrderBy);
            sequence.Orders ??= new List<OrderKeyPart>();
            sequence.Orders.Add(new OrderKeyPart() { Key = expr, Asc = !desc });
        }
        //将序列直接转换为分组模式(不带group by 子句)
        private static ILTSQLObjectQueryable<IGrouping<TKey, T>> AsGroup<TKey, T>(this ILTSQLObjectQueryable<T> src)
        {
            src = src.AsLTSQL();
            SqlQueryPart query = src.SqlQuery.SetNextStep(QueryStepSeq.GroupBy);
            query.GroupFlag = true;
            query.GroupElement = (Expression<Func<T, T>>)(p => p);

            return new LTSQLObject<IGrouping<TKey, T>>(query);
        }



        //初始化查询对象，以支持LINQ语法
        public static ILTSQLOrderedQueryable<T> AsLTSQL<T>(this T obj) where T : class, new()
        {
            return AsLTSQL<T>(obj, null);
        }
        //指定表名
        public static ILTSQLOrderedQueryable<T> AsLTSQL<T>(this T obj, string tableName) where T : class, new()
        {
            TablePart tablePart = new TablePart(typeof(T));
            tablePart.TableName = tableName;

            SqlQueryPart query = new SqlQueryPart();
            query.Step = QueryStepSeq.From;
            query.MappingType = typeof(T);
            query.From = tablePart;

            var ltsql = new LTSQLObject<T>(query);
            return ltsql;
        }
        //指定from为子查询形式的数据源，开启新的外层查询
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


        // 硬编码select字段进行查询如：
        // SELECT 'Mr. liu' as name, 18 as age, 'like books' as Description
        // 批量版本：将使用 union all 连接
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
            ExpressionModifier modifier = new ExpressionModifier();

            Expression newBody = modifier.VisitParameter(expr.Body, p => parameter == p ? constant : p);
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


        //集合化
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

        //多集合共同取并集
        public static ILTSQLObjectSetable<T> UnionSet<T>(this ILTSQLObjectQueryable<T> src, ILTSQLQueryable other, bool distinct = false)
        {
            return AsSet(src, DbSetType.Union, distinct).AppendSet(other);
        }
        public static ILTSQLObjectSetable<T> UnionSet<T>(this ILTSQLObjectSetable<T> src, ILTSQLQueryable other, bool distinct = false)
        {
            return AsSet(src, DbSetType.Union, distinct).AppendSet(other);
        }

        //多集合共同取交集
        public static ILTSQLObjectSetable<T> IntersectSet<T>(this ILTSQLObjectQueryable<T> src, ILTSQLQueryable other, bool distinct = true)
        {
            return AsSet(src, DbSetType.Intersect, distinct).AppendSet(other);
        }
        public static ILTSQLObjectSetable<T> IntersectSet<T>(this ILTSQLObjectSetable<T> src, ILTSQLQueryable other, bool distinct = true)
        {
            return AsSet(src, DbSetType.Intersect, distinct).AppendSet(other);
        }

        //多集合共同取差集
        public static ILTSQLObjectSetable<T> ExceptSet<T>(this ILTSQLObjectQueryable<T> src, ILTSQLQueryable other, bool distinct = true)
        {
            return AsSet(src, DbSetType.Except, distinct).AppendSet(other);
        }
        public static ILTSQLObjectSetable<T> ExceptSet<T>(this ILTSQLObjectSetable<T> src, ILTSQLQueryable other, bool distinct = true)
        {
            return AsSet(src, DbSetType.Except, distinct).AppendSet(other);
        }

        //向当前集合追加相同集合操作, 比如：
        //向并集集合，在追加集合做并集
        //向交集集合，在追加集合做并集
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



        public static ILTSQLOrderedQueryable<T> WithJoin<T>(this ILTSQLObjectQueryable<T> src, JoinType flag)
        {
            return new LTSQLObject<T>(src.SqlQuery) { JoinFlag = flag };
        }
        /// <summary>
        /// 设置联接类型为左连接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static ILTSQLOrderedQueryable<T> WithLeft<T>(this ILTSQLObjectQueryable<T> src)
        {
            return src.WithJoin(JoinType.LeftJoin);
        }
        /// <summary>
        /// 设置联接类型为右联接
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static ILTSQLOrderedQueryable<T> WithRight<T>(this ILTSQLObjectQueryable<T> src)
        {
            return src.WithJoin(JoinType.RightJoin);
        }
        public static ILTSQLOrderedQueryable<T> WithInner<T>(this ILTSQLObjectQueryable<T> src)
        {
            return src.WithJoin(JoinType.InnerJoin);
        }


        public static ILTSQLObjectQueryable<T> Skip<T>(this ILTSQLObjectQueryable<T> src, int skip)
        {
            SqlQueryPart query = src.SqlQuery.SetNextStep(QueryStepSeq.Page);
            query.Skip = skip;
            //主要是，数据库中没有独立使用Skip的场景，所以默认设置一个最大的Take值代替
            if (query.Take == null)
                query.Take = int.MaxValue;

            return new LTSQLObject<T>(query);
        }
        public static ILTSQLObjectQueryable<T> Take<T>(this ILTSQLObjectQueryable<T> src, int take)
        {
            SqlQueryPart query = src.SqlQuery.SetNextStep(QueryStepSeq.Page);
            query.Take = take;
            return new LTSQLObject<T>(query);
        }
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
            query.Wheres ??= new List<Expression>();
            query.Wheres.Add(expr);

            return new LTSQLObject<T>(query);
        }
        //having
        public static ILTSQLObjectQueryable<IGrouping<TKey, T>> Where<T, TKey>(this ILTSQLObjectQueryable<IGrouping<TKey, T>> src, Expression<Func<IGrouping<TKey, T>, bool>> expr)
        {
            SqlQueryPart query = src.SqlQuery.CopyNew() as SqlQueryPart;
            query = query.SetNextStep(QueryStepSeq.Having);
            query.Havings ??= new List<Expression>();
            query.Havings.Add(expr);

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

                ExpressionModifier modifier = new ExpressionModifier();
                Expression _oldPara = lambda.TakeParamter(0);
                Expression _newbody = modifier.ModifyParameter(expr.Body, expr.TakeParamter(0), lambda.Body);
                Expression _newExpr = Expression.Lambda(_newbody, _oldPara as ParameterExpression);
                selectKeyExpr = _newExpr;
            }

            SqlQueryPart _new = (src.SqlQuery.CopyNew() as SqlQueryPart)
               .SetNextStep(QueryStepSeq.Select, true); //连续的select只需要取最后一次

            _new.SelectKey = selectKeyExpr;
            _new.MappingType = typeof(TResult);
            return new LTSQLObject<TResult>(_new);
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


        //直接聚合函数
        public static ILTSQLObjectQueryable<bool> WithAny<T>(this ILTSQLObjectQueryable<T> src)
        {
            src = new LTSQLObject<T>(src.SqlQuery.CopyNew() as SqlQueryPart);
            return AsSelect(() => src.Any());
        }
        public static ILTSQLObjectQueryable<int> WithCount<T>(this ILTSQLObjectQueryable<T> src)
        {
            return src.AsGroup<int, T>().Select(g => g.Count());
        }
        public static ILTSQLObjectQueryable<int> WithCount<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, bool>> selector)
        {
            return WithGroup<T, bool, int>(nameof(Enumerable.Count), src, selector);
        }
        public static ILTSQLObjectQueryable<long> WithLongCount<T>(this ILTSQLObjectQueryable<T> src)
        {
            return src.AsGroup<int, T>().Select(g => g.LongCount());
        }
        public static ILTSQLObjectQueryable<long> WithLongCount<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, bool>> selector)
        {
            return WithGroup<T, bool, long>(nameof(Enumerable.LongCount), src, selector);
        }


        public static ILTSQLObjectQueryable<int> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, int>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }
        public static ILTSQLObjectQueryable<int?> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, int?>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }
        public static ILTSQLObjectQueryable<long> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, long>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }
        public static ILTSQLObjectQueryable<long?> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, long?>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }
        public static ILTSQLObjectQueryable<float> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, float>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }
        public static ILTSQLObjectQueryable<float?> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, float?>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }
        public static ILTSQLObjectQueryable<double> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, double>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }
        public static ILTSQLObjectQueryable<double?> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, double?>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }
        public static ILTSQLObjectQueryable<decimal> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, decimal>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }
        public static ILTSQLObjectQueryable<decimal?> WithSum<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, decimal?>> selector)
        {
            return WithGroup(nameof(Enumerable.Sum), src, selector);
        }


        public static ILTSQLObjectQueryable<int> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, int>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }
        public static ILTSQLObjectQueryable<int?> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, int?>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }
        public static ILTSQLObjectQueryable<long> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, long>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }
        public static ILTSQLObjectQueryable<long?> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, long?>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }
        public static ILTSQLObjectQueryable<float> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, float>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }
        public static ILTSQLObjectQueryable<float?> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, float?>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }
        public static ILTSQLObjectQueryable<double> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, double>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }
        public static ILTSQLObjectQueryable<double?> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, double?>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }
        public static ILTSQLObjectQueryable<decimal> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, decimal>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }
        public static ILTSQLObjectQueryable<decimal?> WithMax<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, decimal?>> selector)
        {
            return WithGroup(nameof(Enumerable.Max), src, selector);
        }


        public static ILTSQLObjectQueryable<int> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, int>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }
        public static ILTSQLObjectQueryable<int?> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, int?>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }
        public static ILTSQLObjectQueryable<long> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, long>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }
        public static ILTSQLObjectQueryable<long?> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, long?>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }
        public static ILTSQLObjectQueryable<float> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, float>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }
        public static ILTSQLObjectQueryable<float?> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, float?>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }
        public static ILTSQLObjectQueryable<double> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, double>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }
        public static ILTSQLObjectQueryable<double?> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, double?>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }
        public static ILTSQLObjectQueryable<decimal> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, decimal>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }
        public static ILTSQLObjectQueryable<decimal?> WithMin<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, decimal?>> selector)
        {
            return WithGroup(nameof(Enumerable.Min), src, selector);
        }


        public static ILTSQLObjectQueryable<double> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, int>> selector)
        {
            return WithGroup<T, int, double>(nameof(Enumerable.Average), src, selector);
        }
        public static ILTSQLObjectQueryable<double?> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, int?>> selector)
        {
            return WithGroup<T, int?, double?>(nameof(Enumerable.Average), src, selector);
        }
        public static ILTSQLObjectQueryable<double> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, long>> selector)
        {
            return WithGroup<T, long, double>(nameof(Enumerable.Average), src, selector);
        }
        public static ILTSQLObjectQueryable<double?> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, long?>> selector)
        {
            return WithGroup<T, long?, double?>(nameof(Enumerable.Average), src, selector);
        }
        public static ILTSQLObjectQueryable<float> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, float>> selector)
        {
            return WithGroup(nameof(Enumerable.Average), src, selector);
        }
        public static ILTSQLObjectQueryable<float?> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, float?>> selector)
        {
            return WithGroup(nameof(Enumerable.Average), src, selector);
        }
        public static ILTSQLObjectQueryable<double> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, double>> selector)
        {
            return WithGroup(nameof(Enumerable.Average), src, selector);
        }
        public static ILTSQLObjectQueryable<double?> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, double?>> selector)
        {
            return WithGroup(nameof(Enumerable.Average), src, selector);
        }
        public static ILTSQLObjectQueryable<decimal> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, decimal>> selector)
        {
            return WithGroup(nameof(Enumerable.Average), src, selector);
        }
        public static ILTSQLObjectQueryable<decimal?> WithAverage<T>(this ILTSQLObjectQueryable<T> src, Expression<Func<T, decimal?>> selector)
        {
            return WithGroup(nameof(Enumerable.Average), src, selector);
        }



        private static ILTSQLObjectQueryable<TResult> WithGroup<T, TResult>(string groupMethodName, ILTSQLObjectQueryable<T> src, Expression<Func<T, TResult>> exprOfSum)
        {
            MethodInfo m = GetEnumerableGroupMethod(groupMethodName, typeof(TResult)).MakeGenericMethod(typeof(T));
            Expression<Func<IGrouping<int, T>, TResult>> expr = BuildGroupMethodExpress<T, TResult, int>(m, exprOfSum);

            return src.AsGroup<int, T>().Select(expr);
        }
        private static ILTSQLObjectQueryable<TResult> WithGroup<T, TValue, TResult>(string groupMethodName, ILTSQLObjectQueryable<T> src, Expression<Func<T, TValue>> exprOfSum)
        {
            MethodInfo m = GetEnumerableGroupMethod(groupMethodName, typeof(TResult)).MakeGenericMethod(typeof(T));
            Expression<Func<IGrouping<int, T>, TResult>> expr = BuildGroupMethodExpress<T, TValue, TResult, int>(m, exprOfSum);

            return src.AsGroup<int, T>().Select(expr);
        }
        private static MethodInfo GetEnumerableGroupMethod(string methodName, Type methodReturnType)
        {
            MethodInfo m = typeof(Enumerable).GetMethods()
                .Where(p => p.Name == methodName && p.IsGenericMethod && p.GetGenericArguments().Length == 1 && p.ReturnType == methodReturnType)
                .Where(p => p.GetParameters().Length == 2 && p.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .First();
            return m;
        }
        private static Expression<Func<IGrouping<TGroupKey, T>, TResult>> BuildGroupMethodExpress<T, TResult, TGroupKey>(MethodInfo groupMethod, Expression<Func<T, TResult>> exprOfGroup)
        {
            ParameterExpression p = Expression.Parameter(typeof(IGrouping<TGroupKey, T>));
            Expression<Func<IGrouping<TGroupKey, T>, TResult>> expr = Expression.Lambda<Func<IGrouping<TGroupKey, T>, TResult>>(
              Expression.Call(null, groupMethod, new Expression[] { p, exprOfGroup }),
              new[] { p }
               );
            return expr;
        }
        private static Expression<Func<IGrouping<TGroupKey, T>, TResult>> BuildGroupMethodExpress<T, TValue, TResult, TGroupKey>(MethodInfo groupMethod, Expression<Func<T, TValue>> exprOfGroup)
        {
            ParameterExpression p = Expression.Parameter(typeof(IGrouping<TGroupKey, T>));
            Expression<Func<IGrouping<TGroupKey, T>, TResult>> expr = Expression.Lambda<Func<IGrouping<TGroupKey, T>, TResult>>(
              Expression.Call(null, groupMethod, new Expression[] { p, exprOfGroup }),
              new[] { p }
               );
            return expr;
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

            LTSQLToken token = tranlator.Translate(q, options);
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
