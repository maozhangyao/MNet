using MNet.LTSQL.Objects;
using MNet.LTSQL.SqlTokenExtends;
using MNet.LTSQL.SqlTokens;
using MNet.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace MNet.LTSQL
{
    public static class LTSQLTokenFactory
    {
        /// <summary>
        /// 构造形如： table.field as field1 的命名语法token
        /// </summary>
        /// <returns></returns>
        public static LTSQLToken CreateAliasToken(LTSQLToken item, string alias)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (alias == null)
                throw new ArgumentNullException(nameof(alias));

            return new AliasToken(item, alias);
        }

        /// <summary>
        /// 构造形如： table.field 的访问语法
        /// </summary>
        /// <returns></returns>
        public static LTSQLToken CreateAccessToken(LTSQLToken obj, string prop, Type propType)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));
            if (propType == null)
                throw new ArgumentNullException(nameof(propType));

            return new AccessPropertyToken(obj, LTSQLTokenFactory.CreateFieldToken(prop, prop, propType));
        }
        public static LTSQLToken CreateAccessToken(LTSQLToken obj, FieldToken prop)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));

            return new AccessPropertyToken(obj, prop);
        }


        public static LTSQLToken CreateFunctionObjectToken(string fName, Type fType = null)
        {
            return CreateObjectToken(SqlObjectType.Function, fName, fType);
        }
        /// <summary>
        /// 构建表对象
        /// </summary>
        /// <param name="objName">在数据库中表示的对象名称</param>
        /// <param name="objType">可空(如果后续支持存储过程，或者函数对象名称时)</param>
        /// <returns></returns>
        public static TableObjectToken CreateTableObjectToken(string objName, TableDescriptor descriptor, Type objType)
        {
            if (objName == null)
                throw new ArgumentNullException(nameof(objName));

            return new TableObjectToken(objName, descriptor, objType);
        }
        
        public static TableRefToken CreateTableRefToken(string alias, TableDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            return new TableRefToken(alias, descriptor);
        }
        public static FieldToken CreateFieldToken(string fieldName, string originFieldName, Type fieldValueType)
        {
            if (fieldName == null)
                throw new ArgumentNullException(nameof(fieldName));

            return new FieldToken(fieldName, originFieldName, fieldValueType);
        }
        /// <summary>
        /// 构建一个对象名称，如：表名
        /// </summary>
        /// <param name="obj">在数据库中表示的对象名称</param>
        /// <param name="objType">可空(如果后续支持存储过程，或者函数对象名称时)</param>
        /// <returns></returns>
        public static ObjectToken CreateObjectToken(SqlObjectType objType, string obj, Type typeOfObj)
        {
            return new ObjectToken(
                    objType
                    , obj
                    , typeOfObj
                );
        }

        /// <summary>
        /// 构造一个对象调用语法token， 如：f(arg1, arg2)
        /// </summary>
        /// <returns></returns>
        public static LTSQLToken CreateCallToken(LTSQLToken obj, LTSQLToken[] parameters)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            LTSQLToken args = null;
            if (parameters.IsNotEmpty())
            {
                args = SequenceToken.CreateWithJoin(
                         parameters,
                         SequenceToken.Create(
                             SyntaxToken.Create(" "),
                             SyntaxToken.Create(",")
                         )
                      );
            }

            if (args != null)
            {
                return SequenceToken.Create(
                          obj,
                          SequenceToken.Create(
                              SyntaxToken.Create("("),
                              args,
                              SyntaxToken.Create(")")
                          )
                       );
            }

            return SequenceToken.Create(
                        obj,
                        SequenceToken.Create(
                              SyntaxToken.Create("("),
                              SyntaxToken.Create(")")
                          )
                   );
        }
        /// <summary>
        /// 构造函数调用
        /// </summary>
        /// <param name="fName"></param>
        /// <param name="parameters"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        public static LTSQLToken CreateFunctionCallToken(string fName, LTSQLToken[] parameters, Type returnType)
        {
            return CreateFunctionCallToken(CreateFunctionObjectToken(fName, returnType), parameters, returnType);
        }
        /// <summary>
        /// 构造函数调用
        /// </summary>
        /// <param name="fName"></param>
        /// <param name="parameters"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        public static LTSQLToken CreateFunctionCallToken(LTSQLToken fName, LTSQLToken[] parameters, Type returnType)
        {
            if (fName == null)
                throw new ArgumentNullException(nameof(fName));

            parameters = parameters ?? new LTSQLToken[0];
            return new FunctionCallToken(fName, parameters, returnType);
        }

        /// <summary>
        /// sql硬编码常量
        /// </summary>
        /// <param name="val"></param>
        /// <param name="db"></param>
        /// <param name="typeOfValue"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static ConstantToken CreateConstantToken(object val, DbTypes db, Type typeOfValue = null)
        {
            if (val == null && typeOfValue == null)
                throw new Exception($"值为null，无法推测出值的类型，请指定{nameof(typeOfValue)}参数");

            string str = DbUtils.ToSqlPart(val, db);
            return new ConstantToken(str, typeOfValue ?? val.GetType());
        }
        public static NullToken CreateNullToken(Type valueTypeOfNull, DbTypes db)
        {
            return new NullToken(valueTypeOfNull)
            {
                Value = DbUtils.ToSqlPart(null, db)
            };
        }
        public static PageToken CreatePageToken(int? skip, int? take)
        {
            return new PageToken(skip, take);
        }
        public static PriorityCalcToken CreatePriorityCalcToken(LTSQLToken inner)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));

            return new PriorityCalcToken(inner);
        }
        public static AddToken CreateAdd(LTSQLToken left, LTSQLToken right, Type typeOfValue)
        {
            return new AddToken(left, right, typeOfValue, true);
        }
        public static SubtractToken CreateSubtract(LTSQLToken left, LTSQLToken right, Type typeOfValue)
        {
            return new SubtractToken(left, right, typeOfValue, true);
        }
        public static DivideToken CreateDivide(LTSQLToken left, LTSQLToken right, Type typeOfValue)
        {
            return new DivideToken(left, right, typeOfValue, true);
        }
        public static MultiplyToken CreateMultiply(LTSQLToken left, LTSQLToken right, Type typeOfValue)
        {
            return new MultiplyToken(left, right, typeOfValue, true);
        }
        public static BinaryToken CreateBinaryToken(string opt, LTSQLToken left, LTSQLToken right, Type typeOfValue)
        {
            return CreateBinaryToken(opt, left, right, typeOfValue, true);
        }
        public static BinaryToken CreateBinaryToken(string opt, LTSQLToken left, LTSQLToken right, Type typeOfValue, bool priority)
        {
            return new BinaryToken(opt, left, right, typeOfValue, priority);
        }
        public static AndToken CreateAndToken(LTSQLToken left, LTSQLToken right)
        {
            return new AndToken(left, right);
        }
        public static OrToken CreateOrToken(LTSQLToken left, LTSQLToken right)
        {
            return new OrToken(left, right);
        }
        public static EqToken CreateEqToken(LTSQLToken left, LTSQLToken right)
        {
            return new EqToken(left, right);
        }
        public static NeqToken CreateNeqToken(LTSQLToken left, LTSQLToken right)
        {
            return new NeqToken(left, right);
        }
        public static GtToken CreateGtToken(LTSQLToken left, LTSQLToken right)
        {
            return new GtToken(left, right);
        }
        public static GeToken CreateGeToken(LTSQLToken left, LTSQLToken right)
        {
            return new GeToken(left, right);
        }
        public static LtToken CreateLtToken(LTSQLToken left, LTSQLToken right)
        {
            return new LtToken(left, right);
        }
        public static LeToken CreateLeToken(LTSQLToken left, LTSQLToken right)
        {
            return new LeToken(left, right);
        }
        public static NotToken CreateNotToken(SqlValueToken valueOfBool)
        {
            return new NotToken(valueOfBool);
        }
        public static IsToken CreateIsToken(LTSQLToken left, LTSQLToken right, bool isNot = false, bool priority = false)
        {
            return new IsToken(left, right, isNot, priority);
        }
        public static LikeToken CreateLikeToken(LTSQLToken left, LTSQLToken right, bool isNot = false, bool priority = false)
        {
            return new LikeToken(left, right, isNot, priority);
        }
        public static InToken CreateInToken(LTSQLToken left, LTSQLToken right, bool isNot = false, bool priority = false)
        {
            return new InToken(left, right, isNot, priority);
        }
        public static SqlParameterToken CreateSqlParameterToken(string pName, object value, Type valueType)
        {
            return new SqlParameterToken(pName, value, valueType);
        }
        public static ClauseToken CreateClauseToken(string clause, params LTSQLToken[] subs)
        {
            return new ClauseToken(clause, subs);
        }
        public static FromClauseToken CreateFromClauseToken(LTSQLToken src)
        {
            return new FromClauseToken(src);
        }
        public static WhereClauseToken CreateWhereClauseToken(LTSQLToken condition)
        {
            return new WhereClauseToken(condition);
        }
        public static OrderByClauseToken CreateOrderByClauseToken(params LTSQLToken[] orderList)
        {
            return new OrderByClauseToken(orderList);
        }
        public static OrderByItemToken CreateOrderByItemToken(LTSQLToken field, bool desc)
        {
            return new OrderByItemToken(field, desc);
        }
        public static GroupClauseToken CreateGroupClauseToken(params LTSQLToken[] groupList)
        {
            return new GroupClauseToken(groupList);
        }
        public static HavingClauseToken CreateHavingClauseToken(LTSQLToken condition)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            return new HavingClauseToken(condition);
        }
        public static TopClauseToken CreateTopClauseToken(LTSQLToken take)
        {
            if (take == null)
                throw new ArgumentNullException(nameof(take));

            return new TopClauseToken(take);
        }
        public static SelectClauseToken CreateSelectClauseToken(params LTSQLToken[] fields)
        {
            return new SelectClauseToken(fields, null, null);
        }
        public static SelectClauseToken CreateSelectClauseToken(LTSQLToken[] fields, LTSQLToken distinct, LTSQLToken topClause)
        {
            return new SelectClauseToken(fields, distinct, topClause);
        }
        public static DistinctToken CreateDistinctToken()
        {
            return new DistinctToken();
        }

        public static SequenceToken CreateSequenceToken(params LTSQLToken[] tokens)
        {
            return SequenceToken.Create(tokens);
        }
        public static ListToken CreateListToken(params LTSQLToken[] tokens)
        {
            return CreateListToken(false, tokens);
        }
        public static ListToken CreateListToken(bool priority, params LTSQLToken[] tokens)
        {
            return new ListToken(tokens, priority);
        }
        public static ListToken CreateListToken(IEnumerable<LTSQLToken> tokens, LTSQLToken append)
        {
            List<LTSQLToken> list = new List<LTSQLToken>(tokens);
            list.Add(append);

            return new ListToken(list);
        }
        public static SwitchCaseToken CreateSwitchCase(LTSQLToken then, LTSQLToken thenValue, LTSQLToken thenElse, Type valueType)
        {
            return new SwitchCaseToken(then, thenValue, thenElse, valueType);
        }
        public static TupleToken CreateTupleToken(ITupleable tuple)
        {
            if (tuple == null)
                throw new ArgumentNullException(nameof(tuple));

            TupleToken token = new TupleToken(tuple.MappingType);
            foreach ((string key, LTSQLToken value) in tuple)
            {
                token.Add(key, value, tuple.GetValueType(key));
            }

            return token;
        }
        public static TupleToken CreateTupleToken(Type mapping)
        {
            return new TupleToken(mapping);
        }
        public static UpdateClauseToken CreateUpdateClauseToken(TableObjectToken table, ITupleable setClause, LTSQLToken whereClause)
        {
            return new UpdateClauseToken(table, (setClause as TupleToken) ?? CreateTupleToken(setClause), whereClause);
        }
        public static JoinToken CreateJoinToken(JoinType joinType, LTSQLToken mainQuery, LTSQLToken joinQuery, LTSQLToken joinKeys)
        {
            if (mainQuery == null)
                throw new ArgumentNullException(nameof(mainQuery));
            if (joinQuery == null)
                throw new ArgumentNullException(nameof(joinQuery));
            if (joinKeys == null)
                throw new ArgumentNullException(nameof(joinKeys));

            return new JoinToken(joinType, mainQuery, joinQuery, joinKeys);
        }
        public static SqlQueryToken CreateSqlQueryToken(TupleToken table, LTSQLToken from, LTSQLToken where, LTSQLToken group, LTSQLToken having, LTSQLToken order, LTSQLToken page, LTSQLToken select, bool priority = false)
        {
            return new SqlQueryToken(table, from, where, group, having, order, page, select, priority);
        }
        public static SetOperationToken CreateSetOperationToken(TupleToken table, IEnumerable<LTSQLToken> querys, DbSetType settype, bool distinct)
        {
            return CreateSetOperationToken(table, querys, settype, distinct, false);
        }
        public static SetOperationToken CreateSetOperationToken(TupleToken table, IEnumerable<LTSQLToken> querys, DbSetType settype, bool distinct, bool priority)
        {
            if (querys == null)
                throw new ArgumentNullException(nameof(querys));

            return new SetOperationToken(table, querys, settype, distinct, priority);
        }
        public static GroupObjToken CreateGroupObjToken(LTSQLToken groupElement, LTSQLToken groupKey)
        {
            if (groupElement == null)
                throw new ArgumentNullException(nameof(groupElement));

            return new GroupObjToken(groupElement, groupKey);
        }

        public static SyntaxToken Syntax(string txt, bool escape = false)
        {
            return SyntaxToken.Create(txt);
        }
    }
}
