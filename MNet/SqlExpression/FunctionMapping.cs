using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace MNet.SqlExpression
{
    public class FunctionMapping
    {
        //类属性或者字段映射到 sql
        //类方法映射到 sql
        public static readonly Dictionary<string, Func<DbType, MemberInfo, string[], string>> FunctionMaps = new Dictionary<string, Func<DbType, MemberInfo, string[], string>>()
        {
            { GetFunctionMapKey(typeof(string).GetProperty(nameof(string.Length))), MapStringLength },
            { GetFunctionMapKey(typeof(List<int>).GetMethod(nameof(List<int>.Contains))), MapListContainer },
            { GetFunctionMapKey(typeof(Enumerable).GetMethods().FirstOrDefault(p => p.Name == "Contains")), MapListContainer }
        };


        /// <summary>
        /// 字符串 Length 属性转换为 SQL 函数
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="lengthProp"></param>
        /// <param name="sqlParts">参数</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string MapStringLength(DbType dbType, MemberInfo lengthProp, string[] sqlParts)
        {
            if (sqlParts.Length != 1)
                throw new ArgumentException("Length函数仅支持一个参数");

            if (dbType == DbType.Mysql)
            {
                return $"LENGTH({sqlParts[0]})";
            }

            throw new ArgumentException($"Length函数暂不支持该数据库({dbType})的映射");
        }

        /// <summary>
        /// 将IEnumerable的Contains方法以及List.Contains 方法转换为 SQL 的 in() 函数
        /// </summary>
        /// <param name="dbTyp"></param>
        /// <param name="containerMethod"></param>
        /// <param name="sqlParts"></param>
        /// <returns></returns>
        public static string MapListContainer(DbType dbTyp, MemberInfo containerMethod, string[] sqlParts)
        {
            return $"{sqlParts[1]} in ({string.Join(',', sqlParts[0])})";
        }

        public static string GetTypeName(Type t)
        {
            //获取成员所在的类型
            string tname = t.Name;
            if (t.IsGenericType)
            {
                //if (t.ContainsGenericParameters)
                //    throw new Exception($"泛型类型需要先确定泛型参数:{t.FullName}");

                //List<string> paras = new List<string>();
                //foreach (Type pType in t.GetGenericArguments())
                //{
                //    paras.Add(GetTypeName(pType));
                //}
                tname += $"[{t.GetGenericArguments().Length}]";
            }

            return tname;
        }
        /// <summary>
        /// 将类成员映射成唯一的一个key
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static string GetFunctionMapKey(MemberInfo member)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            Type t = member.ReflectedType;
            string tname = GetTypeName(t);

            if (member is PropertyInfo prop)
            {
                string name = prop.Name;
                return tname + "." + name;
            }
            else if (member is FieldInfo field)
            {
                string name = field.Name;
                return tname + "." + name;
            }
            else if (member is MethodInfo method)
            {
                string name = method.Name;
                if (method.IsGenericMethod)
                {
                    //if (method.ContainsGenericParameters)
                    //    throw new Exception($"泛型方法需要先确定泛型参数：{t.FullName}.{method.Name}");

                    //List<string> paras = new List<string>();
                    //foreach (Type pType in method.GetGenericArguments())
                    //    paras.Add(GetTypeName(pType));
                    name += $"<{method.GetGenericArguments().Length}>";
                }
                return tname + "." + name;
            }
            throw new Exception("仅支持字段，属性或者函数成员做为映射key");
        }
    }
}
