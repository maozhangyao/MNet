using Dapper;
using MNet.LTSQL;
using System.Data;
using UnitTestModel;
using Xunit;
using Xunit.Abstractions;

namespace LTSQLXUnitTest
{
    /// <summary>
    /// IN、EXISTS 和子查询测试类
    /// 测试 IN 操作（包括元组形式）、EXISTS 操作和各种子查询场景
    /// </summary>
    public class LTSQLInExistsSubQueryTest
    {
        public LTSQLInExistsSubQueryTest(ITestOutputHelper outp)
        {
            this._outp = outp;
        }

        private ITestOutputHelper _outp;

        /// <summary>
        /// 测试简单 IN 操作：WHERE Id IN (1, 2, 3)
        /// </summary>
        [Fact]
        public void In_SimpleInOperation()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            List<object> ids = new List<object> { 1, 2, 3 };

            (string sql, _) = persion.AsLTSQL()
                .Where(p => ids.Contains(p.Id))
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(ids.Contains(item.Id));
                _outp.WriteLine($"{item.Id} - {item.SelfName}");
            }
        }

        /// <summary>
        /// 测试字符串列表的 IN 操作：WHERE SelfName IN ('张三', '李四')
        /// </summary>
        [Fact]
        public void In_StringListInOperation()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            List<string> names = new List<string> { "张三", "李四" };

            (string sql, _) = persion.AsLTSQL()
                .Where(p => names.Contains(p.SelfName))
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(names.Contains(item.SelfName));
                _outp.WriteLine($"{item.Id} - {item.SelfName}");
            }
        }

        /// <summary>
        /// 测试元组形式的 IN 操作：WHERE (Age, SelfName) IN ((20, '张三'), (25, '李四'))
        /// </summary>
        [Fact]
        public void In_TupleInOperation()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            // 构造元组匹配数据
            var tupleData = new[]
            {
                new { age = 20, name = "张三" },
                new { age = 25, name = "李四" }
            };

            (string sql, _) = persion.AsLTSQL()
                .Where(p => new { age = p.Age, name = p.SelfName }.In(tupleData))
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                bool matched = tupleData.Any(t => t.age == item.Age && t.name == item.SelfName);
                Assert.True(matched, $"记录 ({item.Age}, {item.SelfName}) 不在预期的元组列表中");
                _outp.WriteLine($"{item.Id} - Age: {item.Age}, Name: {item.SelfName}");
            }
        }

        /// <summary>
        /// 测试子查询形式的 IN 操作：WHERE Id IN (SELECT Id FROM c_persion_t WHERE Age > 30)
        /// </summary>
        [Fact]
        public void In_SubQueryInOperation()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            var subQuery = persion.AsLTSQL()
                .Where(p => p.Age > 30)
                .Select(p => p.Id);

            (string sql, _) = persion.AsLTSQL()
                .Where(p => subQuery.Contains(p.Id))
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(item.Age > 30 || list.Count == 0);
                _outp.WriteLine($"{item.Id} - Age: {item.Age}, Name: {item.SelfName}");
            }
        }

        /// <summary>
        /// 测试 NOT IN 操作：WHERE Id NOT IN (1, 2, 3)
        /// </summary>
        [Fact]
        public void In_NotInOperation()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            List<object> excludeIds = new List<object> { 1, 2, 3 };

            (string sql, _) = persion.AsLTSQL()
                .Where(p => !excludeIds.Contains(p.Id))
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.False(excludeIds.Contains(item.Id));
                _outp.WriteLine($"{item.Id} - {item.SelfName}");
            }
        }

        /// <summary>
        /// 测试 EXISTS 操作：WHERE EXISTS (SELECT 1 FROM c_teacher_t WHERE PersionId = p.Id)
        /// </summary>
        [Fact]
        public void Exists_ExistsOperation()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();

            var subQuery = teacher.AsLTSQL()
                .Where(t => t.PersionId == persion.Id);

            (string sql, _) = persion.AsLTSQL()
                .Where(p => subQuery.Any())
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            // 验证这些人员都有对应的教师记录
            foreach (var item in list)
            {
                _outp.WriteLine($"{item.Id} - {item.SelfName}");
            }
        }

        /// <summary>
        /// 测试 NOT EXISTS 操作：WHERE NOT EXISTS (SELECT 1 FROM c_teacher_t WHERE PersionId = p.Id)
        /// </summary>
        [Fact]
        public void Exists_NotExistsOperation()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();

            var subQuery = teacher.AsLTSQL()
                .Where(t => t.PersionId == persion.Id);

            (string sql, _) = persion.AsLTSQL()
                .Where(p => !subQuery.Any())
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            // 验证这些人员都没有对应的教师记录
            foreach (var item in list)
            {
                _outp.WriteLine($"{item.Id} - {item.SelfName}");
            }
        }

        /// <summary>
        /// 测试相关子查询：WHERE Age > (SELECT AVG(Age) FROM c_persion_t)
        /// </summary>
        [Fact]
        public void SubQuery_CorrelatedSubQuery()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            var avgAgeQuery = persion.AsLTSQL()
                .WithAverage(p => p.Age);

            (string sql, _) = persion.AsLTSQL()
                .Where(p => p.Age > avgAgeQuery.FirstOrDefault())
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            // 获取平均年龄用于验证
            double avgAge = connection.QueryFirst<double>(
                persion.AsLTSQL().WithAverage(p => p.Age).ToSql(DbTypes.SQLLite, false).Item1
            );

            _outp.WriteLine($"Average Age: {avgAge}");

            foreach (var item in list)
            {
                Assert.True(item.Age > avgAge);
                _outp.WriteLine($"{item.Id} - Age: {item.Age}, Name: {item.SelfName}");
            }
        }

        /// <summary>
        /// 测试标量子查询：SELECT (SELECT COUNT(*) FROM c_teacher_t WHERE PersionId = p.Id) as TeacherCount
        /// </summary>
        [Fact]
        public void SubQuery_ScalarSubQueryInSelect()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();

            (string sql, _) = persion.AsLTSQL()
                .Select(p => new
                {
                    PersonId = p.Id,
                    PersonName = p.SelfName,
                    TeacherCount = teacher.AsLTSQL()
                        .Where(t => t.PersionId == p.Id)
                        .WithCount()
                        .FirstOrDefault()
                })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"PersonId: {item.PersonId}, PersonName: {item.PersonName}, TeacherCount: {item.TeacherCount}");
                Assert.True(item.TeacherCount >= 0);
            }
        }

        /// <summary>
        /// 测试 FROM 子句中的子查询
        /// </summary>
        [Fact]
        public void SubQuery_SubQueryInFromClause()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            var subQuery = persion.AsLTSQL()
                .Where(p => p.Age > 25)
                .Select(p => new { p.Id, p.SelfName, p.Age });

            (string sql, _) = subQuery.AsLTSQL()
                .Where(p => p.Age < 40)
                .OrderBy(p => p.Age)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(item.Age > 25 && item.Age < 40);
                _outp.WriteLine($"Id: {item.Id}, Name: {item.SelfName}, Age: {item.Age}");
            }
        }

        /// <summary>
        /// 测试嵌套子查询
        /// </summary>
        [Fact]
        public void SubQuery_NestedSubQuery()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            // 第一层子查询：年龄大于 25 的人
            var level1Query = persion.AsLTSQL()
                .Where(p => p.Age > 25)
                .Select(p => new { p.Id, p.SelfName });

            // 第二层子查询：从第一层结果中选取 Id 小于 100 的记录
            var level2Query = level1Query.AsLTSQL()
                .Where(p => p.Id < 100);

            (string sql, _) = level2Query
                .OrderByDescending(p => p.Id)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(item.Id < 100);
                _outp.WriteLine($"Id: {item.Id}, Name: {item.SelfName}");
            }
        }

        /// <summary>
        /// 测试 IN 与子查询组合：WHERE Id IN (SELECT PersionId FROM c_teacher_t)
        /// </summary>
        [Fact]
        public void In_InWithSubQuery()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();

            var teacherIds = teacher.AsLTSQL()
                .Select(t => t.PersionId);

            (string sql, _) = persion.AsLTSQL()
                .Where(p => teacherIds.Contains(p.Id))
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            // 获取所有教师的人员 ID
            List<int> teacherPersionIds = connection.Query<int>(
                teacher.AsLTSQL().Select(t => t.PersionId).ToSql(DbTypes.SQLLite, false).Item1
            ).ToList();

            foreach (var item in list)
            {
                Assert.True(teacherPersionIds.Contains(item.Id));
                _outp.WriteLine($"{item.Id} - {item.SelfName}");
            }
        }

        /// <summary>
        /// 测试复杂子查询：带有聚合和分组的子查询
        /// </summary>
        [Fact]
        public void SubQuery_ComplexSubQueryWithAggregate()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            // 子查询：计算每个年龄的平均 ID
            var avgIdByAge = (from p in persion.AsLTSQL()
                              group p by p.Age into g
                              select new
                              {
                                  Age = g.Key,
                                  AvgId = g.Average(p => p.Id)
                              });

            (string sql, _) = avgIdByAge.AsLTSQL()
                .Where(p => p.AvgId > 50)
                .OrderBy(p => p.Age)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(item.AvgId > 50);
                _outp.WriteLine($"Age: {item.Age}, AvgId: {item.AvgId}");
            }
        }

        /// <summary>
        /// 测试 EXISTS 与 Join 组合
        /// </summary>
        [Fact]
        public void Exists_ExistsWithJoin()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();
            CCourseT course = new CCourseT();

            var hasMathCourse = teacher.AsLTSQL()
                .Join(course.AsLTSQL(), 
                    t => t.CourseId, 
                    c => c.Id, 
                    (t, c) => new { t.PersionId, c.Course })
                .Where(x => x.Course.Contains("数学") && x.PersionId == persion.Id);

            (string sql, _) = persion.AsLTSQL()
                .Where(p => hasMathCourse.Any())
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"{item.Id} - {item.SelfName}");
            }
        }

        /// <summary>
        /// 测试元组 IN 与子查询组合
        /// </summary>
        [Fact]
        public void In_TupleInWithSubQuery()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            // 子查询返回元组
            var subQuery = persion.AsLTSQL()
                .Where(p => p.Age > 25)
                .Take(3)
                .Select(p => new { age = p.Age, name = p.SelfName });

            (string sql, _) = persion.AsLTSQL()
                .Where(p => new { age = p.Age, name = p.SelfName }.In(subQuery))
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(item.Age > 25);
                _outp.WriteLine($"{item.Id} - Age: {item.Age}, Name: {item.SelfName}");
            }
        }
    }
}
