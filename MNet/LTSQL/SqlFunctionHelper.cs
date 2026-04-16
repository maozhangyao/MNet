using System;
using System.Text;
using MNet.LTSQL.SqlTokens;

namespace MNet.LTSQL
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
                        .WithFunctionArgs(SequenceToken.CreateWithJoin(
                            new LTSQLToken[]
                            {
                                SyntaxToken.Create("YEAR FROM"),
                                datetime
                            },
                            SyntaxToken.Create(" ")
                            ));
                    break;
                case DbType.SQLLite:

                    LTSQLToken year = DateFormatFunction(db, datetime, LTSQLTokenFactory.CreateConstantToken("%Y", db)).Builder();
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
                        .WithFunctionArgs(SequenceToken.CreateWithJoin(
                            new LTSQLToken[]
                            {
                                SyntaxToken.Create("MONTH FROM"),
                                datetime
                            },
                            SyntaxToken.Create(" ")
                            ));
                    break;
                case DbType.SQLLite:

                    LTSQLToken month = DateFormatFunction(db, datetime, LTSQLTokenFactory.CreateConstantToken("%m", db)).Builder();
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
                        .WithFunctionArgs(SequenceToken.CreateWithJoin(
                            new LTSQLToken[]
                            {
                                SyntaxToken.Create("DAY FROM"),
                                datetime
                            },
                            SyntaxToken.Create(" ")
                            ));
                    break;
                case DbType.SQLLite:

                    LTSQLToken day = DateFormatFunction(db, datetime, LTSQLTokenFactory.CreateConstantToken("%d", db)).Builder();
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
                        .WithFunctionArgs(SequenceToken.CreateWithJoin(
                            new LTSQLToken[]
                            {
                                SyntaxToken.Create("HOUR FROM"),
                                datetime
                            },
                            SyntaxToken.Create(" ")
                            ));
                    break;
                case DbType.SQLLite:

                    LTSQLToken hour = DateFormatFunction(db, datetime, LTSQLTokenFactory.CreateConstantToken("%H", db)).Builder();
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
                        .WithFunctionArgs(SequenceToken.CreateWithJoin(
                            new LTSQLToken[]
                            {
                                SyntaxToken.Create("MINUTE FROM"),
                                datetime
                            },
                            SyntaxToken.Create(" ")
                            ));
                    break;
                case DbType.SQLLite:

                    LTSQLToken minute = DateFormatFunction(db, datetime, LTSQLTokenFactory.CreateConstantToken("%M", db)).Builder();
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
                        .WithFunctionArgs(SequenceToken.CreateWithJoin(
                            new LTSQLToken[]
                            {
                                SyntaxToken.Create("SECOND FROM"),
                                datetime
                            },
                            SyntaxToken.Create(" ")
                            ));
                    break;
                case DbType.SQLLite:

                    LTSQLToken minute = DateFormatFunction(db, datetime, LTSQLTokenFactory.CreateConstantToken("%S", db)).Builder();
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
    }
}
