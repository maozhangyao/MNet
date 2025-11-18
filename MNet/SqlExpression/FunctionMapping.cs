using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MNet.SqlExpression
{
    public class FunctionMapping
    {
        static FunctionMapping()
        {
            FunctionMaps = new Dictionary<string, Func<MemberMappingContext, SqlToken>>
            {
                // string.Length 映射
                { GetFunctionMapKey(typeof(string).GetProperty(nameof(string.Length))), MapStringLength }
                // List.Contains 映射
                ,{ GetFunctionMapKey(typeof(List<int>).GetMethod(nameof(List<int>.Contains))), MapListContainer }
                // IEnumerable<>.Contains 映射
                ,{ GetFunctionMapKey(typeof(Enumerable).GetMethods().FirstOrDefault(p => p.Name == "Contains")), MapListContainer }
                // DbSetExtensions.First 映射
                ,{ GetFunctionMapKey(typeof(DbSetExtensions).GetMethods().FirstOrDefault(p => p.Name == "First")), MapDbSetFirst }
            };
        }

        //类属性或者字段映射到 sql
        //类方法映射到 sql
        public static readonly Dictionary<string, Func<MemberMappingContext, SqlToken>> FunctionMaps;



        //获取类型的映射key(屏蔽掉泛型以及命名空间)
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


        /// <summary>
        /// 字符串 Length 属性转换为 SQL 函数
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="lengthProp"></param>
        /// <param name="sqlParts">参数</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static SqlToken MapStringLength(MemberMappingContext context)
        {
            if (context.Args.Length != 1)
                throw new ArgumentException("Length函数仅支持有且仅有一个参数");

            if (context.SqlOptions?.Db == DbType.Mysql)
            {
                return new SqlToken($"LENGTH({context.Instance.SqlPart})", null, context.Expr);
            }

            throw new ArgumentException($"Length函数暂不支持该数据库({context.SqlOptions?.Db})的映射");
        }
        /// <summary>
        /// 将IEnumerable的Contains方法以及List.Contains 方法转换为 SQL 的 in() 函数
        /// </summary>
        /// <param name="dbTyp"></param>
        /// <param name="containerMethod"></param>
        /// <param name="sqlParts"></param>
        /// <returns></returns>
        public static SqlToken MapListContainer(MemberMappingContext context)
        {
            return new SqlToken($"{context.Args[1].SqlPart} in ({string.Join(',', context.Instance.SqlPart)})", null, context.Expr);
        }
        /// <summary>
        /// 对 DbSetExtensions.First 方法转换成SQL
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static SqlToken MapDbSetFirst(MemberMappingContext context)
        {
            ISqlBuilder builder = new SqlBuilder();
            string sql = builder.Build(context.Args[0].Dynamic, context.SqlOptions);
            return new SqlToken($"({sql})", null, context.Expr);
        }
    }


    /// <summary>
    /// 对象成员映射SQL上下文
    /// </summary>
    public class MemberMappingContext
    {
        public ISqlBuilder SqlBuilder { get; set; }
        public SqlBuildContext BuildContext { get; set; }

        /// <summary>
        /// SQL配置
        /// </summary>
        public SqlOptions SqlOptions { get; set; }
        /// <summary>
        /// 当前翻译过程中对应的表达式节点
        /// </summary>
        public Expression Expr { get; set; }
        /// <summary>
        /// 需要映射的成员
        /// </summary>
        public MemberInfo MappMember { get; set; }
        /// <summary>
        /// 成员所属的对象
        /// </summary>
        public SqlToken Instance { get; set; }
        /// <summary>
        /// 当成员为方法时，所传递的参数
        /// </summary>
        public SqlToken[] Args { get; set; }
        //需要SQL参数化的值
        public SqlParamter AddParameter(object val)
        {
            return null;
        }
    }
}
