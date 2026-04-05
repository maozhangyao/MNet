using System;
using System.Text;
using System.Collections.Generic;
using MNet.LTSQL.v1.SqlTokens;

namespace MNet.LTSQL.v1
{
    public class SqlFunctionHelper
    {
        private static Exception UnknownDb(DbType db)
        {
            return new Exception($"未知的数据库类型枚举值：{db}");
        }

        /// <summary>
        /// 构造 获取日期时间(日期+时间)函数调用
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static FunctionTokenBuilder DateFunction(DbType db)
        {
            string f = db switch
            {
                DbType.Oracle => "SYSDATE",
                DbType.MySQL => "NOW",
                DbType.PGSQL => "NOW",
                DbType.MSSQL => "GETDATE",
                DbType.SQLLite => "DATETIME",
                _ => throw UnknownDb(db)
            };

            var builder = new FunctionTokenBuilder()
                .WithFunctionName(f, typeof(DateTime));

            if (db == DbType.SQLLite)
                builder = builder.WithFunctionArgs(ConstantToken.Create("now", db));

            return builder;
        }
        /// <summary>
        /// 构造 日期时间格式化函数调用
        /// </summary>
        /// <param name="db"></param>
        /// <param name="datetime"></param>
        /// <param name="formatstr"></param>
        /// <returns></returns>
        public static FunctionTokenBuilder DateFormatFunction(DbType db, LTSQLToken datetime, LTSQLToken formatstr)
        {
            string f = db switch
            {
                DbType.Oracle => "TO_CHAR",
                DbType.MySQL => "DATE_FORMAT",
                DbType.PGSQL => "TO_CHAR",
                DbType.MSSQL => "FORMAT",
                DbType.SQLLite => "strftime",
                _ => throw UnknownDb(db)
            };

            var builder = new FunctionTokenBuilder()
             .WithFunctionName(f, typeof(string));

            if (db == DbType.SQLLite)
                builder = builder.WithFunctionArgs(formatstr, datetime);
            else
                builder = builder.WithFunctionArgs(datetime, formatstr);

            return builder;
        }
        /// <summary>
        /// 取出日期时间中的年份
        /// </summary>
        /// <param name="db"></param>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static FunctionTokenBuilder DateYearFunction(DbType db, LTSQLToken datetime)
        {
            FunctionTokenBuilder builder = new FunctionTokenBuilder();
            switch (db)
            {
                case DbType.MySQL:
                case DbType.MSSQL:
                    builder.WithFunctionName("YEAR", typeof(int))
                        .WithFunctionArgs(datetime);
                    break;
                case DbType.Oracle:
                case DbType.PGSQL:
                    builder.WithFunctionName("EXTRACT", typeof(int))
                        .WithFunctionArgs(new TokenItemListToken(
                            new LTSQLToken[]
                            {
                                SyntaxToken.Create("YEAR FROM"),
                                datetime
                            },
                            separator: " "
                            ));
                    break;
                case DbType.SQLLite:

                    LTSQLToken year = DateFormatFunction(db, datetime, ConstantToken.Create("%Y", db)).Builder();
                    builder.WithFunctionName("CAST", typeof(int))
                        .WithFunctionArgs(new TokenItemListToken(
                            new LTSQLToken[]
                            {
                                year,
                                SyntaxToken.Create("AS INTEGER")
                            },
                            separator: " "
                            ));
                    break;
                default:
                    throw UnknownDb(db);
            }

            return builder;
        }
        public static FunctionTokenBuilder DateMonthFunction(DbType db, LTSQLToken datetime)
        {
            FunctionTokenBuilder builder = new FunctionTokenBuilder();
            switch (db)
            {
                case DbType.MySQL:
                case DbType.MSSQL:
                    builder.WithFunctionName("MONTH", typeof(int))
                        .WithFunctionArgs(datetime);
                    break;
                case DbType.Oracle:
                case DbType.PGSQL:
                    builder.WithFunctionName("EXTRACT", typeof(int))
                        .WithFunctionArgs(new TokenItemListToken(
                            new LTSQLToken[]
                            {
                                SyntaxToken.Create("MONTH FROM"),
                                datetime
                            },
                            separator: " "
                            ));
                    break;
                case DbType.SQLLite:

                    LTSQLToken month = DateFormatFunction(db, datetime, ConstantToken.Create("%m", db)).Builder();
                    builder.WithFunctionName("CAST", typeof(int))
                        .WithFunctionArgs(new TokenItemListToken(
                            new LTSQLToken[]
                            {
                                month,
                                SyntaxToken.Create("AS INTEGER")
                            },
                            separator: " "
                            ));
                    break;
                default:
                    throw UnknownDb(db);
            }

            return builder;
        }
        public static FunctionTokenBuilder DateDayFunction(DbType db, LTSQLToken datetime)
        {
            FunctionTokenBuilder builder = new FunctionTokenBuilder();
            switch (db)
            {
                case DbType.MySQL:
                case DbType.MSSQL:
                    builder.WithFunctionName("DAY", typeof(int))
                        .WithFunctionArgs(datetime);
                    break;
                case DbType.Oracle:
                case DbType.PGSQL:
                    builder.WithFunctionName("EXTRACT", typeof(int))
                        .WithFunctionArgs(new TokenItemListToken(
                            new LTSQLToken[]
                            {
                                SyntaxToken.Create("DAY FROM"),
                                datetime
                            },
                            separator: " "
                            ));
                    break;
                case DbType.SQLLite:

                    LTSQLToken day = DateFormatFunction(db, datetime, ConstantToken.Create("%d", db)).Builder();
                    builder.WithFunctionName("CAST", typeof(int))
                        .WithFunctionArgs(new TokenItemListToken(
                            new LTSQLToken[]
                            {
                                day,
                                SyntaxToken.Create("AS INTEGER")
                            },
                            separator: " "
                            ));
                    break;
                default:
                    throw UnknownDb(db);
            }

            return builder;
        }
        public static FunctionTokenBuilder DateHourFunction(DbType db, LTSQLToken datetime)
        {
            FunctionTokenBuilder builder = new FunctionTokenBuilder();
            switch (db)
            {
                case DbType.MySQL:
                    builder.WithFunctionName("HOUR", typeof(int))
                        .WithFunctionArgs(datetime);
                    break;
                case DbType.MSSQL:
                    builder.WithFunctionName("DATEPART", typeof(int))
                        .WithFunctionArgs(SyntaxToken.Create("HOUR"), datetime);
                    break;
                case DbType.Oracle:
                case DbType.PGSQL:
                    builder.WithFunctionName("EXTRACT", typeof(int))
                        .WithFunctionArgs(new TokenItemListToken(
                            new LTSQLToken[]
                            {
                                SyntaxToken.Create("HOUR FROM"),
                                datetime
                            },
                            separator: " "
                            ));
                    break;
                case DbType.SQLLite:

                    LTSQLToken hour = DateFormatFunction(db, datetime, ConstantToken.Create("%H", db)).Builder();
                    builder.WithFunctionName("CAST", typeof(int))
                        .WithFunctionArgs(new TokenItemListToken(
                            new LTSQLToken[]
                            {
                                hour,
                                SyntaxToken.Create("AS INTEGER")
                            },
                            separator: " "
                            ));
                    break;
                default:
                    throw UnknownDb(db);
            }

            return builder;
        }
        public static FunctionTokenBuilder DateMinuteFunction(DbType db, LTSQLToken datetime)
        {
            FunctionTokenBuilder builder = new FunctionTokenBuilder();
            switch (db)
            {
                case DbType.MySQL:
                    builder.WithFunctionName("MINUTE", typeof(int))
                        .WithFunctionArgs(datetime);
                    break;
                case DbType.MSSQL:
                    builder.WithFunctionName("DATEPART", typeof(int))
                        .WithFunctionArgs(SyntaxToken.Create("MINUTE"), datetime);
                    break;
                case DbType.Oracle:
                case DbType.PGSQL:
                    builder.WithFunctionName("EXTRACT", typeof(int))
                        .WithFunctionArgs(new TokenItemListToken(
                            new LTSQLToken[]
                            {
                                SyntaxToken.Create("MINUTE FROM"),
                                datetime
                            },
                            separator: " "
                            ));
                    break;
                case DbType.SQLLite:

                    LTSQLToken minute = DateFormatFunction(db, datetime, ConstantToken.Create("%M", db)).Builder();
                    builder.WithFunctionName("CAST", typeof(int))
                        .WithFunctionArgs(new TokenItemListToken(
                            new LTSQLToken[]
                            {
                                minute,
                                SyntaxToken.Create("AS INTEGER")
                            },
                            separator: " "
                            ));
                    break;
                default:
                    throw UnknownDb(db);
            }

            return builder;
        }
        public static FunctionTokenBuilder DateSecondFunction(DbType db, LTSQLToken datetime)
        {
            FunctionTokenBuilder builder = new FunctionTokenBuilder();
            switch (db)
            {
                case DbType.MySQL:
                    builder.WithFunctionName("SECOND", typeof(int))
                        .WithFunctionArgs(datetime);
                    break;
                case DbType.MSSQL:
                    builder.WithFunctionName("DATEPART", typeof(int))
                        .WithFunctionArgs(SyntaxToken.Create("SECOND"), datetime);
                    break;
                case DbType.Oracle:
                case DbType.PGSQL:
                    builder.WithFunctionName("EXTRACT", typeof(int))
                        .WithFunctionArgs(new TokenItemListToken(
                            new LTSQLToken[]
                            {
                                SyntaxToken.Create("SECOND FROM"),
                                datetime
                            },
                            separator: " "
                            ));
                    break;
                case DbType.SQLLite:

                    LTSQLToken minute = DateFormatFunction(db, datetime, ConstantToken.Create("%S", db)).Builder();
                    builder.WithFunctionName("CAST", typeof(int))
                        .WithFunctionArgs(new TokenItemListToken(
                            new LTSQLToken[]
                            {
                                minute,
                                SyntaxToken.Create("AS INTEGER")
                            },
                            separator: " "
                            ));
                    break;
                default:
                    throw UnknownDb(db);
            }

            return builder;
        }
    }

    public class FunctionTokenBuilder
    {
        private string _funcName;
        private Type _typeOfValue;
        private LTSQLToken[] _funcArgs;
        private Func<LTSQLToken[], Stack<LTSQLToken>> _rangingArgs;
        private Action<Stack<LTSQLToken>, Queue<LTSQLToken>> _takingArgs;


        public FunctionTokenBuilder WithFunctionName(string functionName, Type typeOfValue)
        {
            this._funcName = functionName;
            this._typeOfValue = typeOfValue;
            return this;
        }

        public FunctionTokenBuilder WithFunctionArgs(params LTSQLToken[] args)
        {
            this._funcArgs = args;
            if (args == null)
                return this.UseRecursionCall(null, false);

            return this.UseRecursionCall((all, use) => {
                while (all.Count > 0)
                    use.Enqueue(all.Pop());
            }, reverse: false);
        }

        public FunctionTokenBuilder UseRecursionCall(
            Action<Stack<LTSQLToken>, Queue<LTSQLToken>> takingArgs
            , bool reverse = false
            )
        {
            if (takingArgs == null)
            {
                this._takingArgs = null;
                this._rangingArgs = null;
                return this;
            }

            this._rangingArgs = (args) =>
            {
                Stack<LTSQLToken> stack = new Stack<LTSQLToken>();
                if (reverse)
                {
                    for (int i = 0; i < args.Length; i++)
                        stack.Push(args[i]);
                }
                else
                {
                    for (int i = args.Length - 1; i >= 0; i--)
                        stack.Push(args[i]);
                }
                return stack;
            };

            this._takingArgs = takingArgs;
            return this;
        }

        public FunctionToken Builder()
        {
            FunctionToken func = new FunctionToken(this._funcName, null, _typeOfValue);
            Queue<LTSQLToken> use = new Queue<LTSQLToken>();
            Stack<LTSQLToken> all = this._rangingArgs != null ? this._rangingArgs(this._funcArgs) : new Stack<LTSQLToken>();

            while (all.Count > 0)
            {
                int cnt = all.Count;
                this._takingArgs(all, use);
                if (all.Count != 0 && !(cnt - all.Count >= 1))
                    throw new Exception("递归构造函数调用时，每次参数个数的消耗必须大于等2个。");

                func = new FunctionToken(this._funcName, use.ToArray(), this._typeOfValue);

                if (all.Count <= 0)
                    break;

                all.Push(func);
                use.Clear();
            }

            return func;
        }
    }
}
