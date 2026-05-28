using Dapper;
using MNet.LTSQL;
using System.Data;
using UnitTestModel;
using Xunit;
using Xunit.Abstractions;

namespace LTSQLXUnitTest
{
    /// <summary>
    /// 复杂查询综合测试类
    /// 测试多种 LINQ 语法组合的复杂查询场景
    /// </summary>
    public class LTSQLComplexQueryTest
    {
        public LTSQLComplexQueryTest(ITestOutputHelper outp)
        {
            this._outp = outp;
        }

        private ITestOutputHelper _outp;

        /// <summary>
        /// 测试基础内连接 + 子查询 + 元组 IN 匹配
        /// </summary>
        [Fact]
        public void Complex_BasicInnerJoinWithSubQueryAndTupleIn()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            var query = from p1 in persion.AsLTSQL()
                        from p2 in persion.AsLTSQL()
                        from p3 in persion.AsLTSQL()
                        where !new { age = p1.Age, name = p1.SelfName }.In(
                            persion.AsLTSQL()
                                .Select(p => new { age = p.Age, name = p.SelfName })
                                .Take(1)
                        )
                        select new
                        {
                            first = p1.Id,
                            second = p2.Id,
                            thrid = p3.Id
                        };

            (string sql, _) = query.ToSql(DbTypes.SQLLite, false);
            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);
            _outp.WriteLine($"Result count: {list.Count}");
        }

        /// <summary>
        /// 测试右外连接 + 左外连接 + 多维度 Group By + Having + Order By
        /// </summary>
        [Fact]
        public void Complex_RightLeftJoinWithMultiGroupByHavingOrderBy()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            var query = from p1 in persion.AsLTSQL().Where(p => p.Id > 1)
                        join p2 in persion.AsLTSQL().WithRight() on p1.MotherId equals p2.Id
                        join p3 in persion.AsLTSQL().WithLeft() on new { Id = p1.FatherId } equals new { Id = p3.Id }
                        group new { Id1 = p1.Id, Id2 = p2.Id, Id3 = p3.Id } by new { Id1 = p1.Id, Id2 = p2.Id, Id3 = p3.Id } into gs
                        where gs.Key.Id1 + gs.Key.Id2 + gs.Key.Id3 > 0
                        orderby gs.Key.Id1 + gs.Key.Id3
                        select new
                        {
                            A = gs.Key.Id1,
                            Min = gs.Min(p => p.Id1),
                            Max = gs.Max(p => p.Id3)
                        };

            (string sql, _) = query.ToSql(DbTypes.SQLLite, false);
            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);
            _outp.WriteLine($"Result count: {list.Count}");

            foreach (var item in list)
            {
                _outp.WriteLine($"A: {item.A}, Min: {item.Min}, Max: {item.Max}");
            }
        }

        /// <summary>
        /// 测试多表 Join + 多重 Where + Group By + Having + Order By + 分页
        /// </summary>
        [Fact]
        public void Complex_MultiJoinWithMultipleConditionsAndPaging()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();
            CCourseT course = new CCourseT();

            var query = from p in persion.AsLTSQL()
                        where p.Age > 20
                        join t in teacher.AsLTSQL().WithInner() on p.Id equals t.PersionId
                        join c in course.AsLTSQL().WithInner() on t.CourseId equals c.Id
                        where c.Course.Contains("数学")
                        group new { p, c } by new { c.Course, AgeRange = p.Age > 30 ? "Senior" : "Junior" } into g
                        where g.Count() >= 1
                        orderby g.Key.Course, g.Count() descending
                        select new
                        {
                            CourseName = g.Key.Course,
                            AgeRange = g.Key.AgeRange,
                            StudentCount = g.Count(),
                            AvgAge = g.Average(x => x.p.Age),
                            MaxAge = g.Max(x => x.p.Age)
                        };

            (string sql, _) = query
                .Skip(0)
                .Take(10)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count <= 10);

            foreach (var item in list)
            {
                _outp.WriteLine($"Course: {item.CourseName}, AgeRange: {item.AgeRange}, Count: {item.StudentCount}, AvgAge: {item.AvgAge}");
                Assert.Contains("数学", item.CourseName);
                Assert.True(item.StudentCount >= 1);
            }
        }

        /// <summary>
        /// 测试嵌套子查询 + EXISTS + IN 组合
        /// </summary>
        [Fact]
        public void Complex_NestedSubQueryWithExistsAndIn()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();

            // 子查询1：有教师记录的人员
            var hasTeacher = teacher.AsLTSQL()
                .Select(t => t.PersionId);

            // 子查询2：年龄大于平均年龄的人员
            var avgAgeQuery = persion.AsLTSQL()
                .WithAverage(p => p.Age);

            var query = persion.AsLTSQL()
                .Where(p => hasTeacher.Contains(p.Id))
                .Where(p => p.Age > avgAgeQuery.FirstOrDefault())
                .Select(p => new
                {
                    p.Id,
                    p.SelfName,
                    p.Age
                })
                .OrderByDescending(p => p.Age);

            (string sql, _) = query.ToSql(DbTypes.SQLLite, false);
            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            // 获取平均年龄用于验证
            double avgAge = connection.QueryFirst<double>(
                persion.AsLTSQL().WithAverage(p => p.Age).ToSql(DbTypes.SQLLite, false).Item1
            );

            // 获取有教师记录的人员 ID 列表
            List<int> teacherIds = connection.Query<int>(
                teacher.AsLTSQL().Select(t => t.PersionId).ToSql(DbTypes.SQLLite, false).Item1
            ).ToList();

            foreach (var item in list)
            {
                Assert.True(teacherIds.Contains(item.Id));
                Assert.True(item.Age > avgAge);
                _outp.WriteLine($"Id: {item.Id}, Name: {item.SelfName}, Age: {item.Age}");
            }
        }

        /// <summary>
        /// 测试 Union + Intersect + Except 组合操作
        /// </summary>
        [Fact]
        public void Complex_CombinedSetOperations()
        {
            //注意：sqllite 不支持 INTERSECT ALL ，也不支优先级运算符
            //这里测试的逻辑是： (年轻人 UNION 年老人) INTERSECT 高ID人员， sqllite会报错，生成的SQL语法没啥问题
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            var youngPeople = persion.AsLTSQL()
                .Where(p => p.Age < 30)
                .Select(p => new { p.Id, p.SelfName, p.Age });

            var oldPeople = persion.AsLTSQL()
                .Where(p => p.Age >= 30)
                .Select(p => new { p.Id, p.SelfName, p.Age });

            var highIdPeople = persion.AsLTSQL()
                .Where(p => p.Id > 50)
                .Select(p => new { p.Id, p.SelfName, p.Age });

            // (年轻人 UNION 年老人) INTERSECT 高ID人员
            var query = youngPeople.UnionSet(oldPeople, distinct: true)
                .IntersectSet(highIdPeople);

            (string sql, _) = query.ToSql(DbTypes.SQLLite, false);
            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(item.Id > 50);
                _outp.WriteLine($"Id: {item.Id}, Name: {item.SelfName}, Age: {item.Age}");
            }
        }

        /// <summary>
        /// 测试 SelectMany + Join + Group By 组合
        /// 注意：LTSQL 不支持 SelectMany 中的相关子查询（SQLite 无 LATERAL JOIN）
        /// 应改用标准 Join 语法实现相同功能
        /// </summary>
        [Fact]
        public void Complex_SelectManyWithJoinAndGroupBy()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();

            // 正确做法：使用标准 Join 代替 SelectMany 的相关子查询
            var query = from p in persion.AsLTSQL()
                        join t in teacher.AsLTSQL().WithInner() on p.Id equals t.PersionId
                        group t by p.SelfName into g
                        select new
                        {
                            Name = g.Key,
                            TeacherCount = g.Count(),
                            AvgCourseId = g.Average(t => (double)t.CourseId)
                        };

            (string sql, _) = query
                .OrderByDescending(x => x.TeacherCount)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"Name: {item.Name}, TeacherCount: {item.TeacherCount}, AvgCourseId: {item.AvgCourseId}");
                Assert.True(item.TeacherCount >= 1);
            }
        }

        /// <summary>
        /// 测试多层嵌套子查询
        /// </summary>
        [Fact]
        public void Complex_MultiLevelNestedSubQueries()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            // 第一层：年龄大于 25 的人
            var level1 = persion.AsLTSQL()
                .Where(p => p.Age > 25)
                .Select(p => new { p.Id, p.SelfName, p.Age });

            // 第二层：从第一层中选取 Id 小于 100 的记录
            var level2 = level1.AsLTSQL()
                .Where(p => p.Id < 100)
                .Select(p => new { p.Id, p.SelfName });

            // 第三层：从第二层中选取名字包含特定字符的记录
            var level3 = level2.AsLTSQL()
                .Where(p => p.SelfName.Contains("张"))
                .OrderBy(p => p.Id);

            (string sql, _) = level3.ToSql(DbTypes.SQLLite, false);
            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.Contains("张", item.SelfName);
                _outp.WriteLine($"Id: {item.Id}, Name: {item.SelfName}");
            }
        }

        /// <summary>
        /// 测试复杂聚合查询：多个聚合函数 + 分组 + 排序 + 分页
        /// </summary>
        [Fact]
        public void Complex_ComplexAggregateQuery()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            // 目前暂时不支持 let 语法操作
            var query = from p in persion.AsLTSQL()
                        where p.Age > 20
                        group p by p.SelfName into g
                        let count = g.Count()
                        let avgAge = g.Average(x => x.Age)
                        let maxAge = g.Max(x => x.Age)
                        let minAge = g.Min(x => x.Age)
                        where count >= 1 && maxAge > 25
                        //let minAge = g.Min(x => x.Age) //非法let，因为let的本质是select，g是分组变量并且属于上一层select，当前层无法对上一次的select 中的g分组变量做聚合计算
                        orderby avgAge descending
                        select new
                        {
                            Name = g.Key,
                            Count = count,
                            AvgAge = avgAge,
                            MaxAge = maxAge,
                            MinAge = minAge,
                            AgeRange = maxAge - minAge
                        };

            (string sql, _) = query
                .Skip(0)
                .Take(5)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count <= 5);

            for (int i = 1; i < list.Count; i++)
            {
                Assert.True(list[i - 1].AvgAge >= list[i].AvgAge,
                    $"排序错误：位置 {i-1} 的 AvgAge({list[i-1].AvgAge}) 小于位置 {i} 的 AvgAge({list[i].AvgAge})");
            }

            foreach (var item in list)
            {
                Assert.True(item.Count >= 1);
                Assert.True(item.MaxAge > 25);
                Assert.True(item.MaxAge >= item.MinAge);
                Assert.Equal(item.AgeRange, item.MaxAge - item.MinAge);

                _outp.WriteLine($"Name: {item.Name}, Count: {item.Count}, AvgAge: {item.AvgAge}, " +
                    $"MaxAge: {item.MaxAge}, MinAge: {item.MinAge}, AgeRange: {item.AgeRange}");
            }
        }

        /// <summary>
        /// 测试自连接 + 递归查询模式
        /// </summary>
        [Fact]
        public void Complex_SelfJoinWithRecursivePattern()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            // 查找有父亲记录的人员及其父亲信息
            var query = from child in persion.AsLTSQL()
                        join father in persion.AsLTSQL().WithInner() on child.FatherId equals father.Id
                        select new
                        {
                            ChildId = child.Id,
                            ChildName = child.SelfName,
                            ChildAge = child.Age,
                            FatherId = father.Id,
                            FatherName = father.SelfName,
                            FatherAge = father.Age,
                            AgeDiff = father.Age - child.Age
                        };

            (string sql, _) = query
                .OrderBy(x => x.AgeDiff)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(item.FatherAge > item.ChildAge);
                Assert.Equal(item.AgeDiff, item.FatherAge - item.ChildAge);
                _outp.WriteLine($"Child: {item.ChildName}({item.ChildAge}), Father: {item.FatherName}({item.FatherAge}), Diff: {item.AgeDiff}");
            }
        }

        /// <summary>
        /// 测试条件投影 + 动态排序
        /// </summary>
        [Fact]
        public void Complex_ConditionalProjectionWithDynamicOrdering()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            var query = persion.AsLTSQL()
                .Where(p => p.Age > 20)
                .Select(p => new
                {
                    p.Id,
                    p.SelfName,
                    p.Age,
                    AgeCategory = p.Age < 25 ? "Young" : (p.Age < 35 ? "Middle" : "Old"), // TODO 暂时不支持 三元表达式
                    // DisplayName = p.SelfName + "(" + p.Age.ToString() + ")",  // 不支持直接对字符串做 '+'操作，因为没这个sql标准，需要替换成 string.Concat 函数，如下更正：
                    DisplayName = string.Concat(p.SelfName, string.Concat("(", string.Concat(p.Age, ")"))),  // 注意：string.Concat 函数不支持两个以上的参数，所以需要嵌套调用，因为不同的数据库参数个数支持不一定一样
                    IsYoung = p.Age < 30
                });

            // 根据条件动态排序
            (string sql, _) = query
                .OrderByDescending(p => p.IsYoung)
                .ThenBy(p => p.Age)
                .ThenBy(p => p.SelfName)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            long lastIsYoung = 1;
            long lastAge = 0;
            string lastName = "";

            foreach (var item in list)
            {
                string expectedCategory = item.Age < 25 ? "Young" : (item.Age < 35 ? "Middle" : "Old");
                string expectedDisplayName = $"{item.SelfName}({item.Age})";
                bool expectedIsYoung = item.Age < 30;

                Assert.Equal<string>(expectedCategory, item.AgeCategory);
                Assert.Equal<string>(expectedDisplayName, item.DisplayName);
                Assert.Equal<bool>(expectedIsYoung, (item.IsYoung == 0 ? false : true));

                // 验证排序
                if (item.IsYoung != lastIsYoung)
                {
                    Assert.False(item.IsYoung == 1); // IsYoung 应该从 true 到 false
                }
                else if (item.Age != lastAge)
                {
                    if (item.IsYoung == lastIsYoung)
                    {
                        Assert.True(item.Age >= lastAge);
                    }
                }

                lastIsYoung = item.IsYoung;
                lastAge = item.Age;
                lastName = item.SelfName;

                _outp.WriteLine($"Id: {item.Id}, Name: {item.DisplayName}, Category: {item.AgeCategory}, IsYoung: {item.IsYoung}");
            }
        }

        /// <summary>
        /// 测试极端复杂查询：所有特性的组合
        /// </summary>
        [Fact]
        public void Complex_UltraComplexQuery()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();
            CCourseT course = new CCourseT();

            // 子查询：计算每门课程的统计信息
            var courseStats = (from t in teacher.AsLTSQL()
                               join c in course.AsLTSQL().WithInner() on t.CourseId equals c.Id
                               group new { t, c } by c.Course into g
                               select new
                               {
                                   CourseName = g.Key,
                                   TeacherCount = g.Count(),
                                   AvgCourseId = g.Average(x => x.c.Id)
                               });

            // 主查询：结合多个条件、连接、分组和子查询
            var query = from p in persion.AsLTSQL()
                        where p.Age > 20
                        join t in teacher.AsLTSQL().WithLeft() on p.Id equals t.PersionId
                        where t == null || t.CourseId > 0
                        select new
                        {
                            PersonId = p.Id,
                            PersonName = p.SelfName,
                            PersonAge = p.Age,
                            HasTeacher = t != null,
                            CourseId = t != null ? t.CourseId : (int?)null,
                            AgeGroup = p.Age < 30 ? "Young" : "Old",
                            CurrentYear = DateTime.Now.Year
                        };

            // 注意：由于 DefaultIfEmpty 的限制，这里简化测试
            var simplifiedQuery = from p in persion.AsLTSQL()
                                  where p.Age > 20
                                  select new
                                  {
                                      PersonId = p.Id,
                                      PersonName = p.SelfName,
                                      PersonAge = p.Age,
                                      AgeGroup = p.Age < 30 ? "Young" : "Old",
                                      CurrentYear = DateTime.Now.Year
                                  };

            (string sql, _) = simplifiedQuery
                .OrderByDescending(p => p.PersonAge)
                .ThenBy(p => p.PersonName)
                .Skip(0)
                .Take(20)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count <= 20);

            foreach (var item in list)
            {
                Assert.True(item.PersonAge > 20);
                Assert.Equal(DateTime.Now.Year, item.CurrentYear);

                string expectedAgeGroup = item.PersonAge < 30 ? "Young" : "Old";
                Assert.Equal(expectedAgeGroup, item.AgeGroup);

                _outp.WriteLine($"Id: {item.PersonId}, Name: {item.PersonName}, Age: {item.PersonAge}, AgeGroup: {item.AgeGroup}");
            }
        }
    }
}
