using System;
using System.Runtime.CompilerServices;
using System.Text;
using MNet.LTSQL.SqlTokens;
using MNet.Utils;

namespace MNet.LTSQL
{
    public class SqlFunctionHelper
    {
        // 函数名称
        public const string F_EXISTS = "EXISTS";


        private static Exception UnknownDb(DbTypes db)
        {
            return new Exception($"未知的数据库类型枚举值：{db}");
        }

        /// <summary>
        /// 构造 获取日期时间(日期+时间)函数调用
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static FunctionTokenBuilder DateFunction(DbTypes db)
        {
            string f = db switch
            {
                DbTypes.Oracle => "SYSDATE",
                DbTypes.MySQL => "NOW",
                DbTypes.PGSQL => "NOW",
                DbTypes.MSSQL => "GETDATE",
                DbTypes.SQLLite => "DATETIME",
                _ => throw UnknownDb(db)
            };

            var builder = new FunctionTokenBuilder()
                .WithFunctionName(f, typeof(DateTime));

            if (db == DbTypes.SQLLite)
                builder = builder.WithFunctionArgs(LTSQLTokenFactory.CreateConstantToken("now", db));

            return builder;
        }
        /// <summary>
        /// 构造 日期时间格式化函数调用
        /// </summary>
        /// <param name="db"></param>
        /// <param name="datetime"></param>
        /// <param name="formatstr"></param>
        /// <returns></returns>
        public static FunctionTokenBuilder DateFormatFunction(DbTypes db, LTSQLToken datetime, LTSQLToken formatstr)
        {
            string f = db switch
            {
                DbTypes.Oracle => "TO_CHAR",
                DbTypes.MySQL => "DATE_FORMAT",
                DbTypes.PGSQL => "TO_CHAR",
                DbTypes.MSSQL => "FORMAT",
                DbTypes.SQLLite => "strftime",
                _ => throw UnknownDb(db)
            };

            var builder = new FunctionTokenBuilder()
             .WithFunctionName(f, typeof(string));

            if (db == DbTypes.SQLLite)
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
        public static FunctionTokenBuilder DateYearFunction(DbTypes db, LTSQLToken datetime)
        {
            FunctionTokenBuilder builder = new FunctionTokenBuilder();
            switch (db)
            {
                case DbTypes.MySQL:
                case DbTypes.MSSQL:
                    builder.WithFunctionName("YEAR", typeof(int))
                        .WithFunctionArgs(datetime);
                    break;
                case DbTypes.Oracle:
                case DbTypes.PGSQL:
                    builder.WithFunctionName("EXTRACT", typeof(int))
                        .WithFunctionArgs(SequenceToken.CreateWithJoin(
                            new LTSQLToken[]
                            {
                                SyntaxToken.Create("YEAR FROM"),
                                datetime
                            },
                            SyntaxToken.Create(" ")
                            ));
                    break;
                case DbTypes.SQLLite:

                    LTSQLToken year = DateFormatFunction(db, datetime, LTSQLTokenFactory.CreateConstantToken("%Y", db)).Build();
                    builder.WithFunctionName("CAST", typeof(int))
                        .WithFunctionArgs(SequenceToken.CreateWithJoin(
                            new LTSQLToken[]
                            {
                                year,
                                SyntaxToken.Create("AS INTEGER")
                            },
                            SyntaxToken.Create(" ")
                            ));
                    break;
                default:
                    throw UnknownDb(db);
            }

            return builder;
        }
        public static FunctionTokenBuilder DateMonthFunction(DbTypes db, LTSQLToken datetime)
        {
            FunctionTokenBuilder builder = new FunctionTokenBuilder();
            switch (db)
            {
                case DbTypes.MySQL:
                case DbTypes.MSSQL:
                    builder.WithFunctionName("MONTH", typeof(int))
                        .WithFunctionArgs(datetime);
                    break;
                case DbTypes.Oracle:
                case DbTypes.PGSQL:
                    builder.WithFunctionName("EXTRACT", typeof(int))
                        .WithFunctionArgs(SequenceToken.CreateWithJoin(
                            new LTSQLToken[]
                            {
                                SyntaxToken.Create("MONTH FROM"),
                                datetime
                            },
                            SyntaxToken.Create(" ")
                            ));
                    break;
                case DbTypes.SQLLite:

                    LTSQLToken month = DateFormatFunction(db, datetime, LTSQLTokenFactory.CreateConstantToken("%m", db)).Build();
                    builder.WithFunctionName("CAST", typeof(int))
                        .WithFunctionArgs(SequenceToken.CreateWithJoin(
                            new LTSQLToken[]
                            {
                                month,
                                SyntaxToken.Create("AS INTEGER")
                            },
                            SyntaxToken.Create(" ")
                            ));
                    break;
                default:
                    throw UnknownDb(db);
            }

            return builder;
        }
        public static FunctionTokenBuilder DateDayFunction(DbTypes db, LTSQLToken datetime)
        {
            FunctionTokenBuilder builder = new FunctionTokenBuilder();
            switch (db)
            {
                case DbTypes.MySQL:
                case DbTypes.MSSQL:
                    builder.WithFunctionName("DAY", typeof(int))
                        .WithFunctionArgs(datetime);
                    break;
                case DbTypes.Oracle:
                case DbTypes.PGSQL:
                    builder.WithFunctionName("EXTRACT", typeof(int))
                        .WithFunctionArgs(SequenceToken.CreateWithJoin(
                            new LTSQLToken[]
                            {
                                SyntaxToken.Create("DAY FROM"),
                                datetime
                            },
                            SyntaxToken.Create(" ")
                            ));
                    break;
                case DbTypes.SQLLite:

                    LTSQLToken day = DateFormatFunction(db, datetime, LTSQLTokenFactory.CreateConstantToken("%d", db)).Build();
                    builder.WithFunctionName("CAST", typeof(int))
                        .WithFunctionArgs(SequenceToken.CreateWithJoin(
                            new LTSQLToken[]
                            {
                                day,
                                SyntaxToken.Create("AS INTEGER")
                            },
                            SyntaxToken.Create(" ")
                            ));
                    break;
                default:
                    throw UnknownDb(db);
            }

            return builder;
        }
        public static FunctionTokenBuilder DateHourFunction(DbTypes db, LTSQLToken datetime)
        {
            FunctionTokenBuilder builder = new FunctionTokenBuilder();
            switch (db)
            {
                case DbTypes.MySQL:
                    builder.WithFunctionName("HOUR", typeof(int))
                        .WithFunctionArgs(datetime);
                    break;
                case DbTypes.MSSQL:
                    builder.WithFunctionName("DATEPART", typeof(int))
                        .WithFunctionArgs(SyntaxToken.Create("HOUR"), datetime);
                    break;
                case DbTypes.Oracle:
                case DbTypes.PGSQL:
                    builder.WithFunctionName("EXTRACT", typeof(int))
                        .WithFunctionArgs(SequenceToken.CreateWithJoin(
                            new LTSQLToken[]
                            {
                                SyntaxToken.Create("HOUR FROM"),
                                datetime
                            },
                            SyntaxToken.Create(" ")
                            ));
                    break;
                case DbTypes.SQLLite:

                    LTSQLToken hour = DateFormatFunction(db, datetime, LTSQLTokenFactory.CreateConstantToken("%H", db)).Build();
                    builder.WithFunctionName("CAST", typeof(int))
                        .WithFunctionArgs(SequenceToken.CreateWithJoin(
                            new LTSQLToken[]
                            {
                                hour,
                                SyntaxToken.Create("AS INTEGER")
                            },
                            SyntaxToken.Create(" ")
                            ));
                    break;
                default:
                    throw UnknownDb(db);
            }

            return builder;
        }
        public static FunctionTokenBuilder DateMinuteFunction(DbTypes db, LTSQLToken datetime)
        {
            FunctionTokenBuilder builder = new FunctionTokenBuilder();
            switch (db)
            {
                case DbTypes.MySQL:
                    builder.WithFunctionName("MINUTE", typeof(int))
                        .WithFunctionArgs(datetime);
                    break;
                case DbTypes.MSSQL:
                    builder.WithFunctionName("DATEPART", typeof(int))
                        .WithFunctionArgs(SyntaxToken.Create("MINUTE"), datetime);
                    break;
                case DbTypes.Oracle:
                case DbTypes.PGSQL:
                    builder.WithFunctionName("EXTRACT", typeof(int))
                        .WithFunctionArgs(SequenceToken.CreateWithJoin(
                            new LTSQLToken[]
                            {
                                SyntaxToken.Create("MINUTE FROM"),
                                datetime
                            },
                            SyntaxToken.Create(" ")
                            ));
                    break;
                case DbTypes.SQLLite:

                    LTSQLToken minute = DateFormatFunction(db, datetime, LTSQLTokenFactory.CreateConstantToken("%M", db)).Build();
                    builder.WithFunctionName("CAST", typeof(int))
                        .WithFunctionArgs(SequenceToken.CreateWithJoin(
                            new LTSQLToken[]
                            {
                                minute,
                                SyntaxToken.Create("AS INTEGER")
                            },
                            SyntaxToken.Create(" ")
                            ));
                    break;
                default:
                    throw UnknownDb(db);
            }

            return builder;
        }
        public static FunctionTokenBuilder DateSecondFunction(DbTypes db, LTSQLToken datetime)
        {
            FunctionTokenBuilder builder = new FunctionTokenBuilder();
            switch (db)
            {
                case DbTypes.MySQL:
                    builder.WithFunctionName("SECOND", typeof(int))
                        .WithFunctionArgs(datetime);
                    break;
                case DbTypes.MSSQL:
                    builder.WithFunctionName("DATEPART", typeof(int))
                        .WithFunctionArgs(SyntaxToken.Create("SECOND"), datetime);
                    break;
                case DbTypes.Oracle:
                case DbTypes.PGSQL:
                    builder.WithFunctionName("EXTRACT", typeof(int))
                        .WithFunctionArgs(SequenceToken.CreateWithJoin(
                            new LTSQLToken[]
                            {
                                SyntaxToken.Create("SECOND FROM"),
                                datetime
                            },
                            SyntaxToken.Create(" ")
                            ));
                    break;
                case DbTypes.SQLLite:

                    LTSQLToken minute = DateFormatFunction(db, datetime, LTSQLTokenFactory.CreateConstantToken("%S", db)).Build();
                    builder.WithFunctionName("CAST", typeof(int))
                        .WithFunctionArgs(SequenceToken.CreateWithJoin(
                            new LTSQLToken[]
                            {
                                minute,
                                SyntaxToken.Create("AS INTEGER")
                            },
                            SyntaxToken.Create(" ")
                            ));
                    break;
                default:
                    throw UnknownDb(db);
            }

            return builder;
        }
        public static FunctionTokenBuilder ExistsFunction(DbTypes db, LTSQLToken query)
        {
            FunctionTokenBuilder builder = new FunctionTokenBuilder();
            builder.WithFunctionName(F_EXISTS, typeof(bool))
            .WithFunctionArgs(query);

            return builder;
        }
        /// <summary>
        /// COALESCE 函数，返回首个非空值
        /// </summary>
        /// <param name="db"></param>
        /// <param name="fReturn"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static FunctionTokenBuilder CoalesceFunction(DbTypes db, Type fReturn, params LTSQLToken[] args)
        {
            FunctionTokenBuilder builder = new FunctionTokenBuilder();
            builder.WithFunctionName("COALESCE", fReturn)
            .WithFunctionArgs(args);

            return builder;
        }

        public static FunctionTokenBuilder StringConcatFunction(DbTypes db, params LTSQLToken[] strs)
        {
            if (strs.IsEmpty() || strs.Length < 2)
                throw new Exception("字符串拼接函数入参个数至少为2");

            FunctionTokenBuilder builder = new FunctionTokenBuilder();
            if (db == DbTypes.SQLLite || db == DbTypes.MSSQL || db == DbTypes.Oracle || db == DbTypes.PGSQL)
            {
                builder.WithFunctionName("CONCAT", typeof(string));
                builder.WithFunctionArgs(strs);
                builder.UseRecursionCall((s, q) => {
                    q.Enqueue(s.Pop());
                    q.Enqueue(s.Pop());
                }, false);
            }
            else if (db == DbTypes.MySQL)
            {
                List<LTSQLToken> args = new List<LTSQLToken>();
                args.Add(LTSQLTokenFactory.CreateConstantToken("", db, typeof(string)));
                args.AddRange(args);
             
                builder.WithFunctionName("CONCAT_WS", typeof(string));
                builder.WithFunctionArgs(args.ToArray());
            }
            else
                throw UnknownDb(db);

            return builder;
        }
        public static FunctionTokenBuilder StringLengthFunction(DbTypes db, LTSQLToken str)
        {
            FunctionTokenBuilder builder = new FunctionTokenBuilder();
            if (db == DbTypes.MSSQL)
                builder.WithFunctionName("LEN", typeof(int));
            else if(db == DbTypes.MySQL)
                builder.WithFunctionName("CHAR_LENGTH", typeof(int));
            else
                builder.WithFunctionName("LENGTH", typeof(int));

            return builder.WithFunctionArgs(str);
        }
        public static FunctionTokenBuilder StringSubstrFunction(DbTypes db, LTSQLToken str, LTSQLToken pos, LTSQLToken len)
        {
            FunctionTokenBuilder builder = new FunctionTokenBuilder();
            if (db == DbTypes.SQLLite || db == DbTypes.Oracle)
                builder.WithFunctionName("SUBSTR", typeof(string));
            else if (db == DbTypes.MySQL || db == DbTypes.MSSQL || db == DbTypes.PGSQL)
                builder.WithFunctionName("SUBSTRING", typeof(string));
            else
                throw UnknownDb(db);

            builder.WithFunctionArgs(pos, len);
            return builder;
        }
        public static FunctionTokenBuilder StringTrimLFunction(DbTypes db, LTSQLToken str)
        {
            return new FunctionTokenBuilder().WithFunctionName("LTRIM", typeof(string)).WithFunctionArgs(str);
        }
        public static FunctionTokenBuilder StringTrimRFunction(DbTypes db, LTSQLToken str)
        {
            return new FunctionTokenBuilder().WithFunctionName("RTRIM", typeof(string)).WithFunctionArgs(str);
        }
        public static FunctionTokenBuilder StringTrimFunction(DbTypes db, LTSQLToken str)
        {
            return StringTrimRFunction(db,
                     StringTrimLFunction(db, str).Build()
                );
        }
        public static FunctionTokenBuilder StringLikeLConcat(DbTypes db, LTSQLToken str)
        {
            return StringConcatFunction(db, str, LTSQLTokenFactory.CreateConstantToken("%", db));
        }
        public static FunctionTokenBuilder StringLikeRConcat(DbTypes db, LTSQLToken str)
        {
            return StringConcatFunction(db, LTSQLTokenFactory.CreateConstantToken("%", db), str);
        }
        public static FunctionTokenBuilder StringLikeConcat(DbTypes db, LTSQLToken str)
        {
            return StringConcatFunction(db, LTSQLTokenFactory.CreateConstantToken("%", db), str, LTSQLTokenFactory.CreateConstantToken("%", db));
        }
    }
}
