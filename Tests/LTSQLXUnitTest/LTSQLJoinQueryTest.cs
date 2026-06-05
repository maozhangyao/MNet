using Dapper;
using MNet.LTSQL;
using System.Data;
using UnitTestModel;
using Xunit;
using Xunit.Abstractions;

namespace LTSQLXUnitTest
{
    /// <summary>
    /// JOIN 连接查询测试类
    /// 测试各种类型的连接操作（Inner Join, Left Join, Right Join）和多表连接
    /// </summary>
    public class LTSQLJoinQueryTest
    {
        public LTSQLJoinQueryTest(ITestOutputHelper outp)
        {
            this._outp = outp;
        }

        private ITestOutputHelper _outp;

        /// <summary>
        /// 测试隐式 Inner Join（使用 from...from...where 语法）
        /// </summary>
        [Fact]
        public void Join_ImplicitInnerJoin()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();
            CCourseT course = new CCourseT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               from t in teacher.AsLTSQL()
                               from c in course.AsLTSQL()
                               where p.Id == t.PersionId && t.CourseId == c.Id
                               select new
                               {
                                   PersonId = p.Id,
                                   PersonName = p.SelfName,
                                   CourseName = c.Course
                               })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"PersonId: {item.PersonId}, PersonName: {item.PersonName}, CourseName: {item.CourseName}");
                Assert.NotNull(item.PersonName);
                Assert.NotNull(item.CourseName);
            }
        }

        /// <summary>
        /// 测试显式 Inner Join（使用 join...on 语法）
        /// </summary>
        [Fact]
        public void Join_ExplicitInnerJoin()
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
                                   PersonId = p.Id,
                                   PersonName = p.SelfName,
                                   CourseName = c.Course
                               })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"PersonId: {item.PersonId}, PersonName: {item.PersonName}, CourseName: {item.CourseName}");
                Assert.NotNull(item.PersonName);
                Assert.NotNull(item.CourseName);
            }
        }

        /// <summary>
        /// 测试 Left Join（左外连接）
        /// </summary>
        [Fact]
        public void Join_LeftJoin()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();

            // 使用隐式左连接语法
            (string sql, _) = (from p in persion.AsLTSQL()
                               from t in teacher.AsLTSQL().WithLeft()
                               where p.Id == t.PersionId
                               select new
                               {
                                   PersonId = p.Id,
                                   PersonName = p.SelfName,
                                   TeacherCourseId = t.CourseId
                               })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count > 0);

            foreach (var item in list)
            {
                _outp.WriteLine($"PersonId: {item.PersonId}, PersonName: {item.PersonName}, TeacherCourseId: {item.TeacherCourseId}");
            }
        }

        /// <summary>
        /// 测试 Right Join（右外连接）
        /// </summary>
        [Fact]
        public void Join_RightJoin()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();

            (string sql, _) = (from p in persion.AsLTSQL().WithRight()
                               join t in teacher.AsLTSQL() on p.Id equals t.PersionId
                               select new
                               {
                                   PersonId = p.Id,
                                   PersonName = p.SelfName,
                                   TeacherPersionId = t.PersionId
                               })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"PersonId: {item.PersonId}, PersonName: {item.PersonName}, TeacherPersionId: {item.TeacherPersionId}");
            }
        }

        /// <summary>
        /// 测试多表 Inner Join
        /// </summary>
        [Fact]
        public void Join_MultipleTableInnerJoin()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();
            CCourseT course = new CCourseT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               join t in teacher.AsLTSQL().WithInner() on p.Id equals t.PersionId
                               join c in course.AsLTSQL().WithInner() on t.CourseId equals c.Id
                               where p.Age > 20
                               orderby p.Id
                               select new
                               {
                                   PersonId = p.Id,
                                   PersonName = p.SelfName,
                                   Age = p.Age,
                                   CourseName = c.Course
                               })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"PersonId: {item.PersonId}, PersonName: {item.PersonName}, Age: {item.Age}, CourseName: {item.CourseName}");
                Assert.True(item.Age > 20);
                Assert.NotNull(item.CourseName);
            }
        }

        /// <summary>
        /// 测试 Join 与 Group By 组合
        /// </summary>
        [Fact]
        public void Join_WithGroupBy()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();
            CCourseT course = new CCourseT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               join t in teacher.AsLTSQL().WithInner() on p.Id equals t.PersionId
                               join c in course.AsLTSQL().WithInner() on t.CourseId equals c.Id
                               group new { p, c } by c.Course into g
                               select new
                               {
                                   CourseName = g.Key,
                                   StudentCount = g.Count(),
                                   AvgAge = g.Average(x => x.p.Age)
                               })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"CourseName: {item.CourseName}, StudentCount: {item.StudentCount}, AvgAge: {item.AvgAge}");
                Assert.NotNull(item.CourseName);
                Assert.True(item.StudentCount > 0);
            }
        }

        /// <summary>
        /// 测试 Join 与聚合函数
        /// </summary>
        [Fact]
        public void Join_WithAggregateFunctions()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               join t in teacher.AsLTSQL().WithInner() on p.Id equals t.PersionId
                               select new
                               {
                                   PersonId = p.Id,
                                   PersonName = p.SelfName
                               })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"PersonId: {item.PersonId}, PersonName: {item.PersonName}");
                Assert.NotNull(item.PersonName);
            }
        }

        /// <summary>
        /// 测试 Join 与 Where 条件组合
        /// </summary>
        [Fact]
        public void Join_WithWhereCondition()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();
            CCourseT course = new CCourseT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               join t in teacher.AsLTSQL().WithInner() on p.Id equals t.PersionId
                               join c in course.AsLTSQL().WithInner() on t.CourseId equals c.Id
                               where p.Age > 25 && c.Course.Contains("数学")
                               select new
                               {
                                   PersonId = p.Id,
                                   PersonName = p.SelfName,
                                   Age = p.Age,
                                   CourseName = c.Course
                               })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"PersonId: {item.PersonId}, PersonName: {item.PersonName}, Age: {item.Age}, CourseName: {item.CourseName}");
                Assert.True(item.Age > 25);
                Assert.Contains("数学", item.CourseName);
            }
        }

        /// <summary>
        /// 测试 Join 与 OrderBy 组合
        /// </summary>
        [Fact]
        public void Join_WithOrderBy()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();
            CCourseT course = new CCourseT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               join t in teacher.AsLTSQL().WithInner() on p.Id equals t.PersionId
                               join c in course.AsLTSQL().WithInner() on t.CourseId equals c.Id
                               orderby p.Age descending, p.Id ascending
                               select new
                               {
                                   PersonId = p.Id,
                                   PersonName = p.SelfName,
                                   Age = p.Age,
                                   CourseName = c.Course
                               })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count > 0);

            // 验证排序正确性
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i - 1].Age == list[i].Age)
                {
                    Assert.True(list[i - 1].PersonId <= list[i].PersonId,
                        $"次要排序错误：Age 相同时，位置 {i-1} 的 PersonId({list[i-1].PersonId}) 大于位置 {i} 的 PersonId({list[i].PersonId})");
                }
                else
                {
                    Assert.True(list[i - 1].Age >= list[i].Age,
                        $"主要排序错误：位置 {i-1} 的 Age({list[i-1].Age}) 小于位置 {i} 的 Age({list[i].Age})");
                }
            }
        }

        /// <summary>
        /// 测试自连接（Self Join）
        /// </summary>
        [Fact]
        public void Join_SelfJoin()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = (from p1 in persion.AsLTSQL()
                               join p2 in persion.AsLTSQL().WithInner() on p1.FatherId equals p2.Id
                               select new
                               {
                                   ChildId = p1.Id,
                                   ChildName = p1.SelfName,
                                   FatherId = p2.Id,
                                   FatherName = p2.SelfName
                               })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"ChildId: {item.ChildId}, ChildName: {item.ChildName}, FatherId: {item.FatherId}, FatherName: {item.FatherName}");
                Assert.NotNull(item.ChildName);
                Assert.NotNull(item.FatherName);
            }
        }

        /// <summary>
        /// 测试 SelectMany（扁平化查询）
        /// </summary>
        [Fact]
        public void Join_SelectMany()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();

            (string sql, _) = persion.AsLTSQL()
                .SelectMany(
                    p => teacher.AsLTSQL(), //.Where(t => t.PersionId == p.Id),
                    (p, t) => new
                    {
                        PersonId = p.Id,
                        PersonName = p.SelfName,
                        TeacherCourseId = t.CourseId
                    }
                ).Take(100)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"PersonId: {item.PersonId}, PersonName: {item.PersonName}, TeacherCourseId: {item.TeacherCourseId}");
            }
        }

        /// <summary>
        /// 测试 Join 与分页组合
        /// </summary>
        [Fact]
        public void Join_WithPaging()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();
            CCourseT course = new CCourseT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               join t in teacher.AsLTSQL().WithInner() on p.Id equals t.PersionId
                               join c in course.AsLTSQL().WithInner() on t.CourseId equals c.Id
                               orderby p.Id
                               select new
                               {
                                   PersonId = p.Id,
                                   PersonName = p.SelfName,
                                   CourseName = c.Course
                               })
                .Skip(0)
                .Take(5)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count <= 5);

            foreach (var item in list)
            {
                _outp.WriteLine($"PersonId: {item.PersonId}, PersonName: {item.PersonName}, CourseName: {item.CourseName}");
            }
        }

        /// <summary>
        /// 测试复杂 Join 查询：多表 + 多条件 + 分组 + 排序
        /// </summary>
        [Fact]
        public void Join_ComplexQuery()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();
            CCourseT course = new CCourseT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               where p.Age > 20
                               join t in teacher.AsLTSQL().WithInner() on p.Id equals t.PersionId
                               join c in course.AsLTSQL().WithInner() on t.CourseId equals c.Id
                               group new { p, c } by new { c.Course, p.SelfName } into g
                               where g.Count() >= 1
                               orderby g.Key.Course, g.Key.SelfName
                               select new
                               {
                                   CourseName = g.Key.Course,
                                   PersonName = g.Key.SelfName,
                                   Count = g.Count(),
                                   AvgAge = g.Average(x => x.p.Age)
                               })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"CourseName: {item.CourseName}, PersonName: {item.PersonName}, Count: {item.Count}, AvgAge: {item.AvgAge}");
                Assert.NotNull(item.CourseName);
                Assert.NotNull(item.PersonName);
                Assert.True(item.Count >= 1);
            }
        }
    }
}
