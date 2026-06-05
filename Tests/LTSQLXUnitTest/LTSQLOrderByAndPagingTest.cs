using Dapper;
using MNet.LTSQL;
using System.Data;
using UnitTestModel;
using Xunit;
using Xunit.Abstractions;

namespace LTSQLXUnitTest
{
    /// <summary>
    /// ORDER BY 排序和分页测试类
    /// 测试各种排序方式和分页功能
    /// </summary>
    public class LTSQLOrderByAndPagingTest
    {
        public LTSQLOrderByAndPagingTest(ITestOutputHelper outp)
        {
            this._outp = outp;
        }

        private ITestOutputHelper _outp;

        /// <summary>
        /// 测试单字段升序排序：ORDER BY Id ASC
        /// </summary>
        [Fact]
        public void OrderBy_SingleFieldAscending()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .OrderBy(p => p.Id)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count > 1);

            // 验证排序正确性
            for (int i = 1; i < list.Count; i++)
            {
                Assert.True(list[i - 1].Id <= list[i].Id, 
                    $"排序错误：位置 {i-1} 的 Id({list[i-1].Id}) 大于位置 {i} 的 Id({list[i].Id})");
            }
        }

        /// <summary>
        /// 测试单字段降序排序：ORDER BY Id DESC
        /// </summary>
        [Fact]
        public void OrderBy_SingleFieldDescending()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .OrderByDescending(p => p.Id)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count > 1);

            // 验证排序正确性
            for (int i = 1; i < list.Count; i++)
            {
                Assert.True(list[i - 1].Id >= list[i].Id,
                    $"排序错误：位置 {i-1} 的 Id({list[i-1].Id}) 小于位置 {i} 的 Id({list[i].Id})");
            }
        }

        /// <summary>
        /// 测试多字段排序：ORDER BY Age ASC, Id DESC
        /// </summary>
        [Fact]
        public void OrderBy_MultipleFields()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .OrderBy(p => p.Age)
                .ThenByDescending(p => p.Id)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count > 1);

            // 验证主要排序字段
            for (int i = 1; i < list.Count; i++)
            {
                Assert.True(list[i - 1].Age <= list[i].Age,
                    $"主排序错误：位置 {i-1} 的 Age({list[i-1].Age}) 大于位置 {i} 的 Age({list[i].Age})");
            }
        }

        /// <summary>
        /// 测试多字段复杂排序：ORDER BY SelfName ASC, Age DESC, Id ASC
        /// </summary>
        [Fact]
        public void OrderBy_ComplexMultipleFields()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .OrderBy(p => p.SelfName)
                .ThenByDescending(p => p.Age)
                .ThenBy(p => p.Id)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count > 0);
        }

        /// <summary>
        /// 测试分页 - Skip：LIMIT @p0 OFFSET @p1
        /// </summary>
        [Fact]
        public void Paging_SkipOnly()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .OrderBy(p => p.Id)
                .Skip(5)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            // 获取全部数据用于对比
            List<CPersionT> allList = connection.Query<CPersionT>(
                persion.AsLTSQL().OrderBy(p => p.Id).ToSql(DbTypes.SQLLite, false).Item1
            ).ToList();

            // 验证跳过了前5条
            Assert.True(allList.Count > 5, "数据不足，无法测试 Skip");
            if (list.Count > 0 && allList.Count > 5)
            {
                Assert.Equal(allList[5].Id, list[0].Id);
            }
        }

        /// <summary>
        /// 测试分页 - Take：LIMIT @p0
        /// </summary>
        [Fact]
        public void Paging_TakeOnly()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            int takeCount = 3;
            (string sql, _) = persion.AsLTSQL()
                .OrderBy(p => p.Id)
                .Take(takeCount)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count <= takeCount, $"返回记录数 {list.Count} 超过了限制的 {takeCount}");
        }

        /// <summary>
        /// 测试分页 - Skip + Take：LIMIT @p0 OFFSET @p1
        /// </summary>
        [Fact]
        public void Paging_SkipAndTake()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            int skip = 2;
            int take = 3;
            (string sql, _) = persion.AsLTSQL()
                .OrderBy(p => p.Id)
                .Skip(skip)
                .Take(take)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count <= take, $"返回记录数 {list.Count} 超过了限制的 {take}");

            // 获取全部数据用于对比
            List<CPersionT> allList = connection.Query<CPersionT>(
                persion.AsLTSQL().OrderBy(p => p.Id).ToSql(DbTypes.SQLLite, false).Item1
            ).ToList();

            // 验证分页正确性
            if (list.Count > 0 && allList.Count > skip)
            {
                Assert.Equal(allList[skip].Id, list[0].Id);
            }
        }

        /// <summary>
        /// 测试分页与 Where 条件组合
        /// </summary>
        [Fact]
        public void Paging_WithWhereCondition()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Where(p => p.Age > 20)
                .OrderByDescending(p => p.Age)
                .Skip(1)
                .Take(2)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count <= 2);

            foreach (var item in list)
            {
                Assert.True(item.Age > 20);
            }
        }

        /// <summary>
        /// 测试分页与 Select 投影组合
        /// </summary>
        [Fact]
        public void Paging_WithSelectProjection()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               orderby p.Id
                               select new CPersionSelect1
                               {
                                   Id = p.Id,
                                   Name = p.SelfName
                               })
                .Skip(2)
                .Take(3)
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionSelect1> list = connection.Query<CPersionSelect1>(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count <= 3);

            foreach (var item in list)
            {
                Assert.NotNull(item.Name);
                _outp.WriteLine($"{item.Id} - {item.Name}");
            }
        }

        /// <summary>
        /// 测试 Distinct 去重
        /// </summary>
        [Fact]
        public void OrderBy_Distinct()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Select(p => new { p.Age })
                .Distinct()
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            var list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            // 验证没有重复的 Age 值
            var ages = list.Select(x => x.Age).Cast<long>().ToList();
            Assert.Equal(ages.Count, ages.Distinct().Count());
        }

        /// <summary>
        /// 测试排序与 Group By 组合
        /// </summary>
        [Fact]
        public void OrderBy_WithGroupBy()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = (from p in persion.AsLTSQL()
                               group p by p.SelfName into g
                               orderby g.Key
                               select new CPersionSelect1
                               {
                                   Name = g.Key
                               })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionSelect1> list = connection.Query<CPersionSelect1>(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count > 0);

            // 验证排序正确性
            for (int i = 1; i < list.Count; i++)
            {
                Assert.True(string.Compare(list[i - 1].Name, list[i].Name, StringComparison.Ordinal) <= 0,
                    $"排序错误：位置 {i-1} 的 Name({list[i-1].Name}) 大于位置 {i} 的 Name({list[i].Name})");
            }
        }

        /// <summary>
        /// 测试在子查询中使用排序
        /// </summary>
        [Fact]
        public void OrderBy_InSubQuery()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            var subQuery = persion.AsLTSQL()
                .OrderByDescending(p => p.Id)
                .Take(5);

            (string sql, _) = subQuery.AsLTSQL()
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                })
                .ToSql(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionSelect1> list = connection.Query<CPersionSelect1>(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count <= 5);
        }
    }
}
