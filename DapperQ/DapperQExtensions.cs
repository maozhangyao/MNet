using System;
using MNet.LTSQL;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Reflection;
using Dapper;
using System.Threading;
using System.Data;
using System.Runtime.CompilerServices;

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
        public static T? QueryF<T>(this ILTSQLObjectQueryable<T> qry, IDbConnection connection, Action<LTSQLOptions> configOptioins = null, Action<string> logs = null)
        {
            if (qry == null)
                throw new ArgumentNullException(nameof(qry));
            if(connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (LTSQLOptionsSetting.OptionCreator == null && configOptioins == null)
                throw new Exception($"请配置{nameof(LTSQLOptions)}选配置。可以考虑{nameof(LTSQLOptionsSetting)}全局配置，或者{nameof(configOptioins)}参数。");

            SetAnonymousTypeCache<T>();

            LTSQLOptions option = LTSQLOptionsSetting.OptionCreator != null ? LTSQLOptionsSetting.OptionCreator() : new LTSQLOptions();
            if (configOptioins != null)
                configOptioins(option);

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
        public static async Task<T?> QueryFAsync<T>(this ILTSQLObjectQueryable<T> qry, IDbConnection connection, Action<LTSQLOptions> configOptioins = null, Action<string> logs = null)
        {
            if (qry == null)
                throw new ArgumentNullException(nameof(qry));
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (LTSQLOptionsSetting.OptionCreator == null && configOptioins == null)
                throw new Exception($"请配置{nameof(LTSQLOptions)}选配置。可以考虑{nameof(LTSQLOptionsSetting)}全局配置，或者{nameof(configOptioins)}参数。");

            SetAnonymousTypeCache<T>();

            LTSQLOptions option = LTSQLOptionsSetting.OptionCreator != null ? LTSQLOptionsSetting.OptionCreator() : new LTSQLOptions();
            if (configOptioins != null)
                configOptioins(option);

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
        public static IEnumerable<T> Query<T>(this ILTSQLObjectQueryable<T> qry, IDbConnection connection, Action<LTSQLOptions> configOptioins = null, Action<string> logs = null)
        {
            if (qry == null)
                throw new ArgumentNullException(nameof(qry));
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (LTSQLOptionsSetting.OptionCreator == null && configOptioins == null)
                throw new Exception($"请配置{nameof(LTSQLOptions)}选配置。可以考虑{nameof(LTSQLOptionsSetting)}全局配置，或者{nameof(configOptioins)}参数。");

            SetAnonymousTypeCache<T>();

            LTSQLOptions option = LTSQLOptionsSetting.OptionCreator != null ? LTSQLOptionsSetting.OptionCreator() : new LTSQLOptions();
            if (configOptioins != null)
                configOptioins(option);

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
        public static async Task<IEnumerable<T>> QueryAsync<T>(this ILTSQLObjectQueryable<T> qry, IDbConnection connection, Action<LTSQLOptions> configOptioins = null, Action<string> logs = null)
        {
            if (qry == null)
                throw new ArgumentNullException(nameof(qry));
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (LTSQLOptionsSetting.OptionCreator == null && configOptioins == null)
                throw new Exception($"请配置{nameof(LTSQLOptions)}选配置。可以考虑{nameof(LTSQLOptionsSetting)}全局配置，或者{nameof(configOptioins)}参数。");

            SetAnonymousTypeCache<T>();

            LTSQLOptions option = LTSQLOptionsSetting.OptionCreator != null ? LTSQLOptionsSetting.OptionCreator() : new LTSQLOptions();
            if (configOptioins != null)
                configOptioins(option);

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
