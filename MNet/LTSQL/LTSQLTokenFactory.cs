using MNet.LTSQL.SqlTokens;
using MNet.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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


            return new ObjectAccessToken(obj, prop, propType);
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
        public static LTSQLToken CreateTableObjectToken(string objName, Type objType)
        {
            if (objName == null)
                throw new ArgumentNullException(nameof(objName));
    
            return CreateObjectToken(
                    SqlObjectType.Table
                    , objName
                    , objType
                );
        }
        /// <summary>
        /// 构建一个对象名称，如：表名
        /// </summary>
        /// <param name="obj">在数据库中表示的对象名称</param>
        /// <param name="objType">可空(如果后续支持存储过程，或者函数对象名称时)</param>
        /// <returns></returns>
        public static LTSQLToken CreateObjectToken(SqlObjectType objType, string obj, Type typeOfObj)
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

            if(args != null)
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
        public static ConstantToken CreateConstantToken(object val, DbType db, Type typeOfValue = null)
        {
            if (val == null && typeOfValue == null)
                throw new Exception($"值为null，无法推测出值的类型，请指定{nameof(typeOfValue)}参数");

            string str = DbUtils.ToSqlPart(val, db);
            return new ConstantToken(str, typeOfValue ?? val.GetType());
        }
        public static NullToken CreateNullToken(Type valueTypeOfNull, DbType db)
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
        public static SqlScopeToken CreateSqlScopeToken(LTSQLToken inner)
        {
            if(inner == null)
                throw new ArgumentNullException(nameof(inner));

            return new SqlScopeToken(inner);
        }
        public static PriorityCalcToken CreatePriorityCalcToken(SqlValueToken inner)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));

            return new PriorityCalcToken(inner);
        }

        public static BinaryToken CreateAdd(LTSQLToken left, LTSQLToken right, Type typeOfValue)
        {
            return CreateBinaryToken("+", left, right, typeOfValue);
        }
        public static BinaryToken CreateSubtract(LTSQLToken left, LTSQLToken right, Type typeOfValue)
        {
            return CreateBinaryToken("-", left, right, typeOfValue);
        }
        public static BinaryToken CreateDivide(LTSQLToken left, LTSQLToken right, Type typeOfValue)
        {
            return CreateBinaryToken("/", left, right, typeOfValue);
        }
        public static BinaryToken CreateMultiply(LTSQLToken left, LTSQLToken right, Type typeOfValue)
        {
            return CreateBinaryToken("*", left, right, typeOfValue);
        }
        public static BinaryToken CreateBinaryToken(string opt, LTSQLToken left, LTSQLToken right, Type typeOfValue)
        {
            return new BinaryToken(opt, left, right, typeOfValue);
        }
        public static BoolCalcToken CreateBoolCalcToken(string opt, LTSQLToken left, LTSQLToken right)
        {
            return new BoolCalcToken(left, right, opt);
        }
        public static SqlParameterToken CreateSqlParameterToken(string pName, object value, Type valueType)
        {
            return new SqlParameterToken(pName, value, valueType);
        }
    }
}
