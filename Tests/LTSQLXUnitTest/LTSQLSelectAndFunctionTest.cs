using Dapper;
using MNet.LTSQL;
using System.Data;
using UnitTestModel;
using Xunit;
using Xunit.Abstractions;

namespace LTSQLXUnitTest
{
    /// <summary>
    /// SELECT 投影和函数测试类
    /// 测试各种 SELECT 投影方式、字符串函数、日期时间函数等
    /// </summary>
    public class LTSQLSelectAndFunctionTest
    {
        public LTSQLSelectAndFunctionTest(ITestOutputHelper outp)
        {
            this._outp = outp;
        }

        private ITestOutputHelper _outp;

        /// <summary>
        /// 测试简单字段投影：SELECT Id, SelfName FROM ...
        /// </summary>
        [Fact]
        public void Select_SimpleFieldProjection()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Select(p => new
                {
                    p.Id,
                    p.SelfName
                })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count > 0);

            foreach (var item in list)
            {
                _outp.WriteLine($"Id: {item.Id}, Name: {item.SelfName}");
                Assert.NotNull(item.SelfName);
            }
        }

        /// <summary>
        /// 测试字段重命名投影：SELECT Id as PersonId, SelfName as Name FROM ...
        /// </summary>
        [Fact]
        public void Select_FieldRenamingProjection()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionSelect1> list = connection.Query<CPersionSelect1>(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count > 0);

            foreach (var item in list)
            {
                _outp.WriteLine($"Id: {item.Id}, Name: {item.Name}");
                Assert.NotNull(item.Name);
            }
        }

        /// <summary>
        /// 测试计算字段投影：SELECT Id, Age + 1 as NextAge FROM ...
        /// </summary>
        [Fact]
        public void Select_CalculatedFieldProjection()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Select(p => new
                {
                    p.Id,
                    p.SelfName,
                    NextAge = p.Age + 1
                })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"Id: {item.Id}, Name: {item.SelfName}, NextAge: {item.NextAge}");
                Assert.Equal(item.NextAge, item.Age + 1);
            }
        }

        /// <summary>
        /// 测试条件表达式投影：SELECT Id, CASE WHEN Age > 30 THEN 'Old' ELSE 'Young' END as AgeGroup FROM ...
        /// </summary>
        [Fact]
        public void Select_ConditionalExpressionProjection()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Select(p => new
                {
                    p.Id,
                    p.SelfName,
                    AgeGroup = p.Age > 30 ? "Old" : "Young"
                })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                string expectedGroup = item.Age > 30 ? "Old" : "Young";
                Assert.Equal(expectedGroup, item.AgeGroup);
                _outp.WriteLine($"Id: {item.Id}, Name: {item.SelfName}, AgeGroup: {item.AgeGroup}");
            }
        }

        /// <summary>
        /// 测试字符串拼接：SELECT CONCAT(SelfName, '-', Age) as Info FROM ...
        /// </summary>
        [Fact]
        public void Select_StringConcatenation()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Select(p => new
                {
                    p.Id,
                    Info = p.SelfName + "-" + p.Age.ToString()
                })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                string expectedInfo = $"{item.SelfName}-{item.Age}";
                Assert.Equal(expectedInfo, item.Info);
                _outp.WriteLine($"Id: {item.Id}, Info: {item.Info}");
            }
        }

        /// <summary>
        /// 测试多重字符串拼接：SELECT CONCAT(CONCAT(SelfName, '-'), Course) FROM ...
        /// </summary>
        [Fact]
        public void Select_NestedStringConcatenation()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();
            CCourseT course = new CCourseT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               join t in teacher.AsLTSQL().WithInner() on p.Id equals t.PersionId
                               join c in course.AsLTSQL().WithInner() on t.CourseId equals c.Id
                               select new
                               {
                                   p.Id,
                                   Info = (p.SelfName + "-") + c.Course
                               })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"Id: {item.Id}, Info: {item.Info}");
                Assert.Contains("-", item.Info);
            }
        }

        /// <summary>
        /// 测试日期时间函数 - Year：SELECT strftime('%Y', 'now') as Year
        /// </summary>
        [Fact]
        public void Function_DateTimeYear()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Select(p => new
                {
                    p.Id,
                    Year = DateTime.Now.Year
                })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count > 0);

            int currentYear = DateTime.Now.Year;
            foreach (var item in list)
            {
                Assert.Equal(currentYear, item.Year);
                _outp.WriteLine($"Id: {item.Id}, Year: {item.Year}");
            }
        }

        /// <summary>
        /// 测试日期时间函数 - Month：SELECT strftime('%m', 'now') as Month
        /// </summary>
        [Fact]
        public void Function_DateTimeMonth()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Select(p => new
                {
                    p.Id,
                    Month = DateTime.Now.Month
                })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            int currentMonth = DateTime.Now.Month;
            foreach (var item in list)
            {
                Assert.Equal(currentMonth, item.Month);
                _outp.WriteLine($"Id: {item.Id}, Month: {item.Month}");
            }
        }

        /// <summary>
        /// 测试日期时间函数 - Day：SELECT strftime('%d', 'now') as Day
        /// </summary>
        [Fact]
        public void Function_DateTimeDay()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Select(p => new
                {
                    p.Id,
                    Day = DateTime.Now.Day
                })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            int currentDay = DateTime.Now.Day;
            foreach (var item in list)
            {
                Assert.Equal(currentDay, item.Day);
                _outp.WriteLine($"Id: {item.Id}, Day: {item.Day}");
            }
        }

        /// <summary>
        /// 测试日期时间函数 - Hour：SELECT strftime('%H', 'now') as Hour
        /// </summary>
        [Fact]
        public void Function_DateTimeHour()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Select(p => new
                {
                    p.Id,
                    Hour = DateTime.Now.Hour
                })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            int currentHour = DateTime.Now.Hour;
            foreach (var item in list)
            {
                Assert.Equal(currentHour, item.Hour);
                _outp.WriteLine($"Id: {item.Id}, Hour: {item.Hour}");
            }
        }

        /// <summary>
        /// 测试日期时间函数 - Minute：SELECT strftime('%M', 'now') as Minute
        /// </summary>
        [Fact]
        public void Function_DateTimeMinute()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Select(p => new
                {
                    p.Id,
                    Minute = DateTime.Now.Minute
                })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            int currentMinute = DateTime.Now.Minute;
            foreach (var item in list)
            {
                Assert.Equal(currentMinute, item.Minute);
                _outp.WriteLine($"Id: {item.Id}, Minute: {item.Minute}");
            }
        }

        /// <summary>
        /// 测试日期时间函数 - Second：SELECT strftime('%S', 'now') as Second
        /// </summary>
        [Fact]
        public void Function_DateTimeSecond()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Select(p => new
                {
                    p.Id,
                    Second = DateTime.Now.Second
                })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            int currentSecond = DateTime.Now.Second;
            foreach (var item in list)
            {
                // 允许 1 秒误差（由于 SQL 执行时间）
                Assert.True(Math.Abs(item.Second - currentSecond) <= 1);
                _outp.WriteLine($"Id: {item.Id}, Second: {item.Second}");
            }
        }

        /// <summary>
        /// 测试多个日期时间函数组合
        /// </summary>
        [Fact]
        public void Function_MultipleDateTimeFunctions()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Select(p => new
                {
                    p.Id,
                    Year = DateTime.Now.Year,
                    Month = DateTime.Now.Month,
                    Day = DateTime.Now.Day,
                    Hour = DateTime.Now.Hour,
                    Minute = DateTime.Now.Minute,
                    Second = DateTime.Now.Second
                })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count > 0);

            var now = DateTime.Now;
            foreach (var item in list)
            {
                Assert.Equal(now.Year, item.Year);
                Assert.Equal(now.Month, item.Month);
                Assert.Equal(now.Day, item.Day);
                Assert.Equal(now.Hour, item.Hour);
                Assert.Equal(now.Minute, item.Minute);
                Assert.True(Math.Abs(item.Second - now.Second) <= 1);

                _outp.WriteLine($"Id: {item.Id}, DateTime: {item.Year}-{item.Month}-{item.Day} {item.Hour}:{item.Minute}:{item.Second}");
            }
        }

        /// <summary>
        /// 测试字符串格式化函数：SELECT strftime('%Y %m-%H', 'now')
        /// </summary>
        [Fact]
        public void Function_DateTimeFormatString()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Select(p => new
                {
                    p.Id,
                    FormattedDate = DateTime.Now.ToString("%Y %m-%H")
                })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"Id: {item.Id}, FormattedDate: {item.FormattedDate}");
                Assert.NotNull(item.FormattedDate);
            }
        }

        /// <summary>
        /// 测试复杂投影：包含计算、条件和字符串操作
        /// </summary>
        [Fact]
        public void Select_ComplexProjection()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Select(p => new
                {
                    p.Id,
                    DisplayName = p.SelfName + "(" + p.Age.ToString() + ")",
                    AgeGroup = p.Age > 30 ? "Senior" : "Junior",
                    DoubleAge = p.Age * 2,
                    CurrentYear = DateTime.Now.Year
                })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                string expectedDisplayName = $"{item.SelfName}({item.Age})";
                string expectedAgeGroup = item.Age > 30 ? "Senior" : "Junior";
                
                Assert.Equal(expectedDisplayName, item.DisplayName);
                Assert.Equal(expectedAgeGroup, item.AgeGroup);
                Assert.Equal(item.DoubleAge, item.Age * 2);
                Assert.Equal(DateTime.Now.Year, item.CurrentYear);

                _outp.WriteLine($"Id: {item.Id}, DisplayName: {item.DisplayName}, AgeGroup: {item.AgeGroup}");
            }
        }

        /// <summary>
        /// 测试 AsSelect 方法（硬编码值查询）
        /// </summary>
        [Fact]
        public void Select_AsSelectHardCodedValues()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();

            // 使用表达式方式创建硬编码查询
            var testData = new HardCodedTestData { Name = "Mr. liu", Age = 18, Description = "like books" };
            (string sql, _) = testData.AsSelect().ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);
            Assert.Single(list);

            var item = list[0];
            Assert.Equal("Mr. liu", item.Name);
            Assert.Equal(18, item.Age);
            Assert.Equal("like books", item.Description);

            _outp.WriteLine($"Name: {item.Name}, Age: {item.Age}, Description: {item.Description}");
        }

        /// <summary>
        /// 用于测试 AsSelect 的辅助类
        /// </summary>
        private class HardCodedTestData
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Description { get; set; }
        }

        /// <summary>
        /// 测试投影与 Where 条件组合
        /// </summary>
        [Fact]
        public void Select_ProjectionWithWhere()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Where(p => p.Age > 25)
                .Select(p => new
                {
                    p.Id,
                    p.SelfName,
                    p.Age,
                    IsSenior = p.Age > 30
                })
                .OrderByDescending(p => p.Age)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(item.Age > 25);
                bool expectedIsSenior = item.Age > 30;
                Assert.Equal(expectedIsSenior, item.IsSenior);

                _outp.WriteLine($"Id: {item.Id}, Name: {item.SelfName}, Age: {item.Age}, IsSenior: {item.IsSenior}");
            }
        }

        /// <summary>
        /// 测试投影与 Group By 组合
        /// </summary>
        [Fact]
        public void Select_ProjectionWithGroupBy()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               group p by p.SelfName into g
                               select new
                               {
                                   Name = g.Key,
                                   Count = g.Count(),
                                   AvgAge = g.Average(x => x.Age),
                                   MaxAge = g.Max(x => x.Age),
                                   MinAge = g.Min(x => x.Age)
                               })
                .OrderByDescending(p => p.Count)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(item.Count > 0);
                Assert.True(item.MaxAge >= item.MinAge);
                Assert.True(item.AvgAge >= item.MinAge && item.AvgAge <= item.MaxAge);

                _outp.WriteLine($"Name: {item.Name}, Count: {item.Count}, AvgAge: {item.AvgAge}, MaxAge: {item.MaxAge}, MinAge: {item.MinAge}");
            }
        }
    }
}
