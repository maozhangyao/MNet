using Dapper;
using MNet.LTSQL;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DapperQ
{
    public static class DapperQExtensions
    {
        private class TypeMapCache<T>
        {
            public static int Flag = 0;
        }
        private static object _lock = new object();

        private static void SetAnonymousTypeCache<T>()
        {
            Type t = typeof(T);
            ConstructorInfo[] ctors = t.GetConstructors(); //返回所有公开的构造
            if (ctors == null || ctors.Length < 1)
                throw new Exception($"类型{t.FullName}没有公开的构造函数。");

            ConstructorInfo? explicitCtor = ctors.FirstOrDefault(p => p.GetParameters().Length <= 0);
            if (explicitCtor == null && ctors.Length > 1)
                throw new Exception($"类型{t.FullName}没有合法构造函数。");

            if (explicitCtor == null && TypeMapCache<T>.Flag == 0)
            {
                lock (_lock)
                {
                    if (TypeMapCache<T>.Flag == 0)
                        SqlMapper.SetTypeMap(t, new AnonymousTypeMap(t));
                    TypeMapCache<T>.Flag = 1;
                }
            }
        }
        private static void EmitLog(Action<string> logs, string sql, List<(string key, object val)>? parameters)
        {
            if (logs == null)
                return;
            if (parameters == null || parameters.Count < 1)
            {
                logs(sql);
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach((string key, object val) in parameters)
            {
                sb.AppendLine($"[{key}, {val}]");
            }
            sb.AppendLine();
            sb.Append(sql);

            logs(sb.ToString());
        }


        // 仅仅读取第一行记录
        public static T? QueryF<T>(this ILTSQLObjectQueryable<T> qry)
        {
            SqlContext? ctx = qry.Query.Follow as SqlContext;
            if (ctx == null)
                throw new Exception($"未检测到{nameof(SqlContext)}信息，请使用{nameof(SqlContext)}.{nameof(SqlContext.Create)}创建查询表达式。");

            return qry.QueryF(ctx.Connection, ctx.Options!, ctx.Log);
        }
        public static async Task<T?> QueryFAsync<T>(this ILTSQLObjectQueryable<T> qry)
        {
            SqlContext? ctx = qry.Query.Follow as SqlContext;
            if (ctx == null)
                throw new Exception($"未检测到{nameof(SqlContext)}信息，请使用{nameof(SqlContext)}.{nameof(SqlContext.Create)}创建查询表达式。");

            return await qry.QueryFAsync(ctx.Connection, ctx.Options!, ctx.Log);
        }
        public static T? QueryF<T>(this ILTSQLObjectQueryable<T> qry, IDbConnection connection, Action<LTSQLOptions> configOptions, Action<string> logs = null)
        {
            LTSQLOptions option = LTSQLOptionsSetting.OptionCreator != null ? LTSQLOptionsSetting.OptionCreator() : new LTSQLOptions();
            configOptions(option);
            return qry.QueryF(connection, option, logs);
        }
        public static async Task<T?> QueryFAsync<T>(this ILTSQLObjectQueryable<T> qry, IDbConnection connection, Action<LTSQLOptions> configOptions, Action<string> logs = null)
        {
            LTSQLOptions option = LTSQLOptionsSetting.OptionCreator != null ? LTSQLOptionsSetting.OptionCreator() : new LTSQLOptions();
            configOptions(option);
            return await qry.QueryFAsync(connection, option, logs);
        }
        public static T? QueryF<T>(this ILTSQLObjectQueryable<T> qry, IDbConnection connection, LTSQLOptions options = null, Action<string> logs = null)
        {
            if (qry == null)
                throw new ArgumentNullException(nameof(qry));
            if(connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (LTSQLOptionsSetting.OptionCreator == null && options == null)
                throw new Exception($"请配置{nameof(LTSQLOptions)}选配置。可以考虑{nameof(LTSQLOptionsSetting)}全局配置，或者{nameof(options)}参数。");

            SetAnonymousTypeCache<T>();

            LTSQLOptions option = options ?? LTSQLOptionsSetting.OptionCreator();
            (string sql, var parameters) = qry.ToSqlWithParameter(option, null);

            if(logs != null)
            {
                EmitLog(logs, sql, option.UseSqlParameter ? parameters : null);
            }

            if (option.UseSqlParameter)
            {
                //只读取第一行
                Dictionary<string, object> dic = parameters.ToDictionary(p => p.key, p => p.val);
                return connection.QueryFirstOrDefault<T>(sql, dic);
            }

            return connection.QueryFirstOrDefault<T>(sql);
        }
        // 仅仅读取第一行记录
        public static async Task<T?> QueryFAsync<T>(this ILTSQLObjectQueryable<T> qry, IDbConnection connection, LTSQLOptions options = null, Action<string> logs = null)
        {
            if (qry == null)
                throw new ArgumentNullException(nameof(qry));
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (LTSQLOptionsSetting.OptionCreator == null && options == null)
                throw new Exception($"请配置{nameof(LTSQLOptions)}选配置。可以考虑{nameof(LTSQLOptionsSetting)}全局配置，或者{nameof(options)}参数。");

            SetAnonymousTypeCache<T>();

            LTSQLOptions option = options ?? LTSQLOptionsSetting.OptionCreator!();
            (string sql, var parameters) = qry.ToSqlWithParameter(option, null);

            if (logs != null)
            {
                EmitLog(logs, sql, option.UseSqlParameter ? parameters : null);
            }

            if (option.UseSqlParameter)
            {
                //只读取第一行
                Dictionary<string, object> dic = parameters.ToDictionary(p => p.key, p => p.val);
                return await connection.QueryFirstOrDefaultAsync<T>(sql, dic);
            }

            return await connection.QueryFirstOrDefaultAsync<T>(sql);
        }



        // 返回所有查询结果
        public static IEnumerable<T> Query<T>(this ILTSQLObjectQueryable<T> qry)
        {
            SqlContext? ctx = qry.Query.Follow as SqlContext;
            if (ctx == null)
                throw new Exception($"未检测到{nameof(SqlContext)}信息，请使用{nameof(SqlContext)}.{nameof(SqlContext.Create)}创建查询表达式。");

            return qry.Query(ctx.Connection, ctx.Options!, ctx.Log);
        }
        public static async Task<IEnumerable<T>> QueryAsync<T>(this ILTSQLObjectQueryable<T> qry)
        {
            SqlContext? ctx = qry.Query.Follow as SqlContext;
            if (ctx == null)
                throw new Exception($"未检测到{nameof(SqlContext)}信息，请使用{nameof(SqlContext)}.{nameof(SqlContext.Create)}创建查询表达式。");

            return await qry.QueryAsync(ctx.Connection, ctx.Options!, ctx.Log);
        }
        public static IEnumerable<T> Query<T>(this ILTSQLObjectQueryable<T> qry, IDbConnection connection, Action<LTSQLOptions> configOptions, Action<string> logs = null)
        {
            LTSQLOptions option = LTSQLOptionsSetting.OptionCreator != null ? LTSQLOptionsSetting.OptionCreator() : new LTSQLOptions();
            configOptions(option);
            return qry.Query(connection, option, logs);
        }
        public static async Task<IEnumerable<T>> QueryAsync<T>(this ILTSQLObjectQueryable<T> qry, IDbConnection connection, Action<LTSQLOptions> configOptions, Action<string> logs = null)
        {
            LTSQLOptions option = LTSQLOptionsSetting.OptionCreator != null ? LTSQLOptionsSetting.OptionCreator() : new LTSQLOptions();
            configOptions(option);
            return await qry.QueryAsync(connection, option, logs);
        }
        public static IEnumerable<T> Query<T>(this ILTSQLObjectQueryable<T> qry, IDbConnection connection, LTSQLOptions options = null, Action<string> logs = null)
        {
            if (qry == null)
                throw new ArgumentNullException(nameof(qry));
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (LTSQLOptionsSetting.OptionCreator == null && options == null)
                throw new Exception($"请配置{nameof(LTSQLOptions)}选配置。可以考虑{nameof(LTSQLOptionsSetting)}全局配置，或者{nameof(options)}参数。");

            SetAnonymousTypeCache<T>();

            LTSQLOptions option = options ?? LTSQLOptionsSetting.OptionCreator!();
            (string sql, var parameters) = qry.ToSqlWithParameter(option, null);

            if (logs != null)
            {
                EmitLog(logs, sql, option.UseSqlParameter ? parameters : null);
            }

            if (option.UseSqlParameter)
            {
                //只读取第一行
                Dictionary<string, object> dic = parameters.ToDictionary(p => p.key, p => p.val);
                return connection.Query<T>(sql, dic);
            }

            return connection.Query<T>(sql);
        }
        // 返回所有查询结果
        public static async Task<IEnumerable<T>> QueryAsync<T>(this ILTSQLObjectQueryable<T> qry, IDbConnection connection, LTSQLOptions options = null, Action<string> logs = null)
        {
            if (qry == null)
                throw new ArgumentNullException(nameof(qry));
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (LTSQLOptionsSetting.OptionCreator == null && options == null)
                throw new Exception($"请配置{nameof(LTSQLOptions)}选配置。可以考虑{nameof(LTSQLOptionsSetting)}全局配置，或者{nameof(options)}参数。");

            SetAnonymousTypeCache<T>();

            LTSQLOptions option = options ?? LTSQLOptionsSetting.OptionCreator!();
            (string sql, var parameters) = qry.ToSqlWithParameter(option, null);

            if (logs != null)
            {
                EmitLog(logs, sql, option.UseSqlParameter ? parameters : null);
            }

            if (option.UseSqlParameter)
            {
                //只读取第一行
                Dictionary<string, object> dic = parameters.ToDictionary(p => p.key, p => p.val);
                return await connection.QueryAsync<T>(sql, dic);
            }

            return await connection.QueryAsync<T>(sql);
        }
    }
}