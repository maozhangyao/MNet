using Dapper;
using MNet.LTSQL;
using System.Data;
using UnitTestModel;
using Xunit;
using Xunit.Abstractions;

namespace LTSQLXUnitTest
{
    /// <summary>
    /// GROUP BY 分组和聚合函数测试类
    /// 测试各种分组查询和聚合函数（Count, Sum, Max, Min, Average）
    /// </summary>
    public class LTSQLGroupByAndAggregateTest
    {
        public LTSQLGroupByAndAggregateTest(ITestOutputHelper outp)
        {
            this._outp = outp;
        }

        private ITestOutputHelper _outp;

        /// <summary>
        /// 测试单字段分组：GROUP BY SelfName
        /// </summary>
        [Fact]
        public void GroupBy_SingleField()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               group p by p.SelfName into g
                               select new
                               {
                                   Name = g.Key,
                                   Count = g.Count()
                               })
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count > 0);

            foreach (var item in list)
            {
                _outp.WriteLine($"Name: {item.Name}, Count: {item.Count}");
                Assert.NotNull(item.Name);
                Assert.True(item.Count > 0);
            }
        }

        /// <summary>
        /// 测试多字段分组：GROUP BY Age, SelfName
        /// </summary>
        [Fact]
        public void GroupBy_MultipleFields()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               group p by new { p.Age, p.SelfName } into g
                               select new
                               {
                                   Age = g.Key.Age,
                                   Name = g.Key.SelfName,
                                   Count = g.Count()
                               })
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count > 0);

            foreach (var item in list)
            {
                _outp.WriteLine($"Age: {item.Age}, Name: {item.Name}, Count: {item.Count}");
                Assert.True(item.Count > 0);
            }
        }

        /// <summary>
        /// 测试分组后使用 HAVING 过滤：HAVING COUNT(*) > 1
        /// </summary>
        [Fact]
        public void GroupBy_WithHaving()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               group p by p.SelfName into g
                               where g.Count() > 1
                               select new
                               {
                                   Name = g.Key,
                                   Count = g.Count()
                               })
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"Name: {item.Name}, Count: {item.Count}");
                Assert.True(item.Count > 1, $"HAVING 条件未生效：Count 应该大于 1，实际为 {item.Count}");
            }
        }

        /// <summary>
        /// 测试分组与 Where 条件组合
        /// </summary>
        [Fact]
        public void GroupBy_WithWhereCondition()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               where p.Age > 20
                               group p by p.SelfName into g
                               select new
                               {
                                   Name = g.Key,
                                   Count = g.Count(),
                                   AvgAge = g.Average(p => p.Age)
                               })
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"Name: {item.Name}, Count: {item.Count}, AvgAge: {item.AvgAge}");
                Assert.True(item.Count > 0);
                Assert.True(item.AvgAge > 20);
            }
        }

        /// <summary>
        /// 测试 Count 聚合函数
        /// </summary>
        [Fact]
        public void Aggregate_Count()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Select(p => new
                {
                    TotalCount = persion.AsLTSQL().Count()
                })
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            // 直接使用 WithCount 方法
            (string sql2, _) = persion.AsLTSQL()
                .WithCount()
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL with WithCount: {sql2}");

            int count = connection.QueryFirst<int>(sql2);
            Assert.True(count > 0);
            _outp.WriteLine($"Total Count: {count}");
        }

        /// <summary>
        /// 测试带条件的 Count 聚合函数
        /// </summary>
        [Fact]
        public void Aggregate_CountWithCondition()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .WithCount(p => p.Age > 30)
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            int count = connection.QueryFirst<int>(sql);
            Assert.True(count >= 0);
            _outp.WriteLine($"Count(Age > 30): {count}");
        }

        /// <summary>
        /// 测试 Sum 聚合函数
        /// </summary>
        [Fact]
        public void Aggregate_Sum()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .WithSum(p => p.Id)
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            int sum = connection.QueryFirst<int>(sql);
            Assert.True(sum > 0);
            _outp.WriteLine($"Sum of Id: {sum}");
        }

        /// <summary>
        /// 测试 Max 聚合函数
        /// </summary>
        [Fact]
        public void Aggregate_Max()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .WithMax(p => p.Age)
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            int max = connection.QueryFirst<int>(sql);
            Assert.True(max > 0);
            _outp.WriteLine($"Max Age: {max}");

            // 验证最大值正确性
            List<CPersionT> all = connection.Query<CPersionT>(
                persion.AsLTSQL().ToSqlWithParameter(DbTypes.SQLLite, false).Item1
            ).ToList();
            
            int expectedMax = all.Max(p => p.Age);
            Assert.Equal(expectedMax, max);
        }

        /// <summary>
        /// 测试 Min 聚合函数
        /// </summary>
        [Fact]
        public void Aggregate_Min()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .WithMin(p => p.Age)
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            int min = connection.QueryFirst<int>(sql);
            Assert.True(min >= -1);
            _outp.WriteLine($"Min Age: {min}");

            // 验证最小值正确性
            List<CPersionT> all = connection.Query<CPersionT>(
                persion.AsLTSQL().ToSqlWithParameter(DbTypes.SQLLite, false).Item1
            ).ToList();

            int expectedMin = all.Min(p => p.Age);
            Assert.Equal(expectedMin, min);
        }

        /// <summary>
        /// 测试 Average 聚合函数
        /// </summary>
        [Fact]
        public void Aggregate_Average()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .WithAverage(p => p.Age)
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            double avg = connection.QueryFirst<double>(sql);
            Assert.True(avg > 0);
            _outp.WriteLine($"Average Age: {avg}");

            // 验证平均值正确性
            List<CPersionT> all = connection.Query<CPersionT>(
                persion.AsLTSQL().ToSqlWithParameter(DbTypes.SQLLite, false).Item1
            ).ToList();

            double expectedAvg = all.Average(p => p.Age);
            Assert.Equal(expectedAvg, avg, 2); // 允许小数点后2位误差
        }

        /// <summary>
        /// 测试 LongCount 聚合函数
        /// </summary>
        [Fact]
        public void Aggregate_LongCount()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .WithLongCount()
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            long count = connection.QueryFirst<long>(sql);
            Assert.True(count > 0);
            _outp.WriteLine($"LongCount: {count}");
        }

        /// <summary>
        /// 测试分组中的多个聚合函数组合
        /// </summary>
        [Fact]
        public void Aggregate_MultipleInGroup()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               group p by p.SelfName into g
                               select new
                               {
                                   Name = g.Key,
                                   Count = g.Count(),
                                   SumId = g.Sum(p => p.Id),
                                   MaxAge = g.Max(p => p.Age),
                                   MinAge = g.Min(p => p.Age),
                                   AvgAge = g.Average(p => p.Age)
                               })
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count > 0);

            foreach (var item in list)
            {
                _outp.WriteLine($"Name: {item.Name}, Count: {item.Count}, SumId: {item.SumId}, " +
                    $"MaxAge: {item.MaxAge}, MinAge: {item.MinAge}, AvgAge: {item.AvgAge}");
                
                Assert.True(item.Count > 0);
                Assert.True(item.SumId >= 0);
                Assert.True(item.MaxAge >= item.MinAge);
                Assert.True(item.AvgAge >= item.MinAge && item.AvgAge <= item.MaxAge);
            }
        }

        /// <summary>
        /// 测试分组后排序
        /// </summary>
        [Fact]
        public void GroupBy_WithOrderBy()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               group p by p.SelfName into g
                               orderby g.Count() descending
                               select new
                               {
                                   Name = g.Key,
                                   Count = g.Count()
                               })
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count > 0);

            // 验证按 Count 降序排列
            for (int i = 1; i < list.Count; i++)
            {
                Assert.True(list[i - 1].Count >= list[i].Count,
                    $"排序错误：位置 {i-1} 的 Count({list[i-1].Count}) 小于位置 {i} 的 Count({list[i].Count})");
            }
        }

        /// <summary>
        /// 测试复杂分组：带有 Where、Having 和 OrderBy
        /// </summary>
        [Fact]
        public void GroupBy_ComplexQuery()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               where p.Id > 0
                               group p by p.SelfName into g
                               where g.Count() >= 1 && g.Max(p => p.Age) > 20
                               orderby g.Key
                               select new
                               {
                                   Name = g.Key,
                                   Count = g.Count(),
                                   MaxAge = g.Max(p => p.Age),
                                   MinAge = g.Min(p => p.Age)
                               })
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"Name: {item.Name}, Count: {item.Count}, MaxAge: {item.MaxAge}, MinAge: {item.MinAge}");
                Assert.True(item.Count >= 1);
                Assert.True(item.MaxAge > 20);
                Assert.True(item.MaxAge >= item.MinAge);
            }
        }
    }
}
