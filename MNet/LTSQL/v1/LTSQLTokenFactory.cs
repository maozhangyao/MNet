using MNet.LTSQL.v1.SqlTokens;
using MNet.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace MNet.LTSQL.v1
{
    public static class LTSQLTokenFactory
    {
        /// <summary>
        /// 构造形如： table.field as field1 的命名语法token
        /// </summary>
        /// <returns></returns>
        public static LTSQLToken CreateAliasToken(LTSQLToken item, LTSQLToken itemAlias)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (itemAlias == null)
                throw new ArgumentNullException(nameof(itemAlias));

            return SequenceToken.Create(
                    item,
                    SyntaxToken.Create("AS"),
                    itemAlias
                );
        }
        /// <summary>
        /// 构造形如： table.field as field1 的命名语法token
        /// </summary>
        /// <returns></returns>
        public static LTSQLToken CreateAliasToken(LTSQLToken item, string itemAlias)
        {
            return CreateAliasToken(item, SyntaxToken.Create(itemAlias, true));
        }

        /// <summary>
        /// 构造形如： table.field 的访问语法
        /// </summary>
        /// <returns></returns>
        public static LTSQLToken CreateAccessToken(LTSQLToken obj, LTSQLToken prop, Type propType)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));
            if (propType == null)
                throw new ArgumentNullException(nameof(propType));


            return new ObjectAccessToken(
                    SequenceToken.Create(
                        obj,
                        SyntaxToken.Create("."),
                        prop
                    ),
                    propType
                );
        }
        /// <summary>
        /// 构造形如： table.field 的访问语法
        /// </summary>
        /// <returns></returns>
        public static LTSQLToken CreateAccessToken(LTSQLToken obj, string prop, Type propType)
        {
            return CreateAccessToken(obj, SyntaxToken.Create(prop, true), propType);
        }

        /// <summary>
        /// 构建一个对象名称，如：表名
        /// </summary>
        /// <param name="objName">在数据库中表示的对象名称</param>
        /// <param name="objType">可空(如果后续支持存储过程，或者函数对象名称时)</param>
        /// <returns></returns>
        public static LTSQLToken CreateObjectToken(string objName, Type objType)
        {
            if (objName == null)
                throw new ArgumentNullException(nameof(objName));

            return CreateObjectToken(
                    objName
                    , objType
                    , true
                );
        }
        /// <summary>
        /// 构建一个对象名称，如：表名
        /// </summary>
        /// <param name="objName">在数据库中表示的对象名称</param>
        /// <param name="objType">可空(如果后续支持存储过程，或者函数对象名称时)</param>
        /// <param name="escapeKey"></param>
        /// <returns></returns>
        public static LTSQLToken CreateObjectToken(string objName, Type objType, bool escapeKey)
        {
            if (objName == null)
                throw new ArgumentNullException(nameof(objName));

            return CreateObjectToken(
                    SyntaxToken.Create(objName, escapeKey)
                    , objType
                );
        }
        /// <summary>
        /// 构建一个对象名称，如：表名
        /// </summary>
        /// <param name="obj">在数据库中表示的对象名称</param>
        /// <param name="objType">可空(如果后续支持存储过程，或者函数对象名称时)</param>
        /// <returns></returns>
        public static LTSQLToken CreateObjectToken(LTSQLToken obj, Type objType)
        {
            return new ObjectToken(
                    obj
                    , objType
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
            return CreateFunctionCallToken(SyntaxToken.Create(fName, false), parameters, returnType);
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

            return new FunctionCallToken(CreateCallToken(fName, parameters), returnType);
        }
    }
}
