using Dapper;
using MNet.LTSQL;
using System.Data;
using UnitTestModel;
using Xunit;
using Xunit.Abstractions;

namespace LTSQLXUnitTest
{
    /// <summary>
    /// 集合操作测试类
    /// 测试 Union、Intersect、Except 等集合操作
    /// </summary>
    public class LTSQLSetOperationTest
    {
        public LTSQLSetOperationTest(ITestOutputHelper outp)
        {
            this._outp = outp;
        }

        private ITestOutputHelper _outp;

        /// <summary>
        /// 测试 UNION ALL 操作（不去重）
        /// </summary>
        [Fact]
        public void SetOperation_UnionAll()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            var query1 = persion.AsLTSQL()
                .Where(p => p.Id <= 5)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            var query2 = persion.AsLTSQL()
                .Where(p => p.Id >= 3 && p.Id <= 8)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            (string sql, _) = query1.UnionSet(query2, distinct: false).ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionSelect1> list = connection.Query<CPersionSelect1>(sql).ToList();
            Assert.NotNull(list);

            // UNION ALL 应该包含重复记录
            _outp.WriteLine($"Total records (with duplicates): {list.Count}");
            
            foreach (var item in list)
            {
                _outp.WriteLine($"{item.Id} - {item.Name}");
            }
        }

        /// <summary>
        /// 测试 UNION DISTINCT 操作（去重）
        /// </summary>
        [Fact]
        public void SetOperation_UnionDistinct()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            var query1 = persion.AsLTSQL()
                .Where(p => p.Id <= 5)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            var query2 = persion.AsLTSQL()
                .Where(p => p.Id >= 3 && p.Id <= 8)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            (string sql, _) = query1.UnionSet(query2, distinct: true).ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionSelect1> list = connection.Query<CPersionSelect1>(sql).ToList();
            Assert.NotNull(list);

            // UNION DISTINCT 应该去除重复记录
            var ids = list.Select(x => x.Id).ToList();
            Assert.Equal(ids.Count, ids.Distinct().Count());

            _outp.WriteLine($"Total records (distinct): {list.Count}");
            foreach (var item in list)
            {
                _outp.WriteLine($"{item.Id} - {item.Name}");
            }
        }

        /// <summary>
        /// 测试 INTERSECT 操作（交集）
        /// </summary>
        [Fact]
        public void SetOperation_Intersect()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            var query1 = persion.AsLTSQL()
                .Where(p => p.Id <= 5)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            var query2 = persion.AsLTSQL()
                .Where(p => p.Id >= 3 && p.Id <= 8)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            (string sql, _) = query1.IntersectSet(query2, true).ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionSelect1> list = connection.Query<CPersionSelect1>(sql).ToList();
            Assert.NotNull(list);

            // INTERSECT 应该只返回两个查询的共同记录（Id 在 3-5 之间）
            foreach (var item in list)
            {
                Assert.True(item.Id >= 3 && item.Id <= 5, 
                    $"INTERSECT 结果错误：Id={item.Id} 不在预期范围 [3, 5] 内");
                _outp.WriteLine($"{item.Id} - {item.Name}");
            }
        }

        /// <summary>
        /// 测试 EXCEPT 操作（差集）
        /// </summary>
        [Fact]
        public void SetOperation_Except()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            var query1 = persion.AsLTSQL()
                .Where(p => p.Id <= 5)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            var query2 = persion.AsLTSQL()
                .Where(p => p.Id >= 3 && p.Id <= 8)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            (string sql, _) = query1.ExceptSet(query2).ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionSelect1> list = connection.Query<CPersionSelect1>(sql).ToList();
            Assert.NotNull(list);

            // EXCEPT 应该返回 query1 中有但 query2 中没有的记录（Id 在 1-2 之间）
            foreach (var item in list)
            {
                Assert.True(item.Id < 3, 
                    $"EXCEPT 结果错误：Id={item.Id} 不应该在 query2 的范围 [3, 8] 内");
                _outp.WriteLine($"{item.Id} - {item.Name}");
            }
        }

        /// <summary>
        /// 测试多个集合的 Union 操作
        /// </summary>
        [Fact]
        public void SetOperation_MultipleUnion()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            var query1 = persion.AsLTSQL()
                .Where(p => p.Id <= 3)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            var query2 = persion.AsLTSQL()
                .Where(p => p.Id >= 3 && p.Id <= 6)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            var query3 = persion.AsLTSQL()
                .Where(p => p.Id >= 6 && p.Id <= 9)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            (string sql, _) = query1.UnionSet(query2, distinct: true)
                .AppendSet(query3)
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionSelect1> list = connection.Query<CPersionSelect1>(sql).ToList();
            Assert.NotNull(list);

            // 验证结果是三个查询的并集（去重）
            var ids = list.Select(x => x.Id).OrderBy(x => x).ToList();
            _outp.WriteLine($"Ids: {string.Join(", ", ids)}");
            
            // 应该包含 1-9 的所有 Id
            for (int i = 1; i <= 9; i++)
            {
                Assert.Contains(i, ids);
            }
        }

        /// <summary>
        /// 测试集合操作与 Where 条件组合
        /// </summary>
        [Fact]
        public void SetOperation_UnionWithWhere()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            var query1 = persion.AsLTSQL()
                .Where(p => p.Age > 25)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            var query2 = persion.AsLTSQL()
                .Where(p => p.Age < 35)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            (string sql, _) = query1.UnionSet(query2, distinct: true).ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionSelect1> list = connection.Query<CPersionSelect1>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"{item.Id} - {item.Name}");
            }
        }

        /// <summary>
        /// 测试 Intersect 与 Group By 组合
        /// </summary>
        [Fact]
        public void SetOperation_IntersectWithGroupBy()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            var query1 = (from p in persion.AsLTSQL()
                          where p.Age > 20
                          group p by p.SelfName into g
                          select new
                          {
                              Name = g.Key,
                              Count = g.Count()
                          });

            var query2 = (from p in persion.AsLTSQL()
                          where p.Age < 40
                          group p by p.SelfName into g
                          select new
                          {
                              Name = g.Key,
                              Count = g.Count()
                          });

            (string sql, _) = query1.IntersectSet(query2, true).ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            dynamic list = connection.Query(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                _outp.WriteLine($"Name: {item.Name}, Count: {item.Count}");
                Assert.NotNull(item.Name);
            }
        }

        /// <summary>
        /// 测试复杂集合操作：Union + Intersect 组合
        /// </summary>
        [Fact]
        public void SetOperation_ComplexSetOperations()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            var query1 = persion.AsLTSQL()
                .Where(p => p.Id <= 5)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            var query2 = persion.AsLTSQL()
                .Where(p => p.Id >= 3 && p.Id <= 7)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            var query3 = persion.AsLTSQL()
                .Where(p => p.Id >= 5 && p.Id <= 10)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            // (query1 UNION query2) INTERSECT query3
            (string sql, _) = query1.UnionSet(query2, distinct: true)
                .IntersectSet(query3)
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionSelect1> list = connection.Query<CPersionSelect1>(sql).ToList();
            Assert.NotNull(list);

            // 结果应该是 query3 与 (query1 UNION query2) 的交集
            // 即 Id 在 5-7 之间的记录
            foreach (var item in list)
            {
                Assert.True(item.Id >= 5 && item.Id <= 7,
                    $"结果错误：Id={item.Id} 不在预期范围 [5, 7] 内");
                _outp.WriteLine($"{item.Id} - {item.Name}");
            }
        }

        /// <summary>
        /// 测试 Except 的反向操作
        /// </summary>
        [Fact]
        public void SetOperation_ExceptReverse()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            var query1 = persion.AsLTSQL()
                .Where(p => p.Id <= 5)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            var query2 = persion.AsLTSQL()
                .Where(p => p.Id >= 3 && p.Id <= 8)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            // query2 EXCEPT query1（与之前的测试相反）
            (string sql, _) = query2.ExceptSet(query1).ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionSelect1> list = connection.Query<CPersionSelect1>(sql).ToList();
            Assert.NotNull(list);

            // 应该返回 query2 中有但 query1 中没有的记录（Id 在 6-8 之间）
            foreach (var item in list)
            {
                Assert.True(item.Id > 5,
                    $"EXCEPT 结果错误：Id={item.Id} 应该在 query1 的范围 [1, 5] 之外");
                _outp.WriteLine($"{item.Id} - {item.Name}");
            }
        }

        /// <summary>
        /// 测试集合操作中的 Distinct 标志
        /// </summary>
        [Fact]
        public void SetOperation_DistinctFlagComparison()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            var query1 = persion.AsLTSQL()
                .Where(p => p.Id <= 3)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            var query2 = persion.AsLTSQL()
                .Where(p => p.Id >= 2 && p.Id <= 4)
                .Select(p => new CPersionSelect1
                {
                    Id = p.Id,
                    Name = p.SelfName
                });

            // UNION ALL（不去重）
            (string sqlAll, _) = query1.UnionSet(query2, distinct: false).ToSqlWithParameter(DbTypes.SQLLite, false);

            // UNION DISTINCT（去重）
            (string sqlDistinct, _) = query1.UnionSet(query2, distinct: true).ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL UNION ALL: {sqlAll}");
            _outp.WriteLine($"SQL UNION DISTINCT: {sqlDistinct}");

            List<CPersionSelect1> listAll = connection.Query<CPersionSelect1>(sqlAll).ToList();
            List<CPersionSelect1> listDistinct = connection.Query<CPersionSelect1>(sqlDistinct).ToList();

            // UNION ALL 的记录数应该大于或等于 UNION DISTINCT
            Assert.True(listAll.Count >= listDistinct.Count,
                $"UNION ALL 的记录数({listAll.Count})应该大于或等于 UNION DISTINCT 的记录数({listDistinct.Count})");

            _outp.WriteLine($"UNION ALL count: {listAll.Count}");
            _outp.WriteLine($"UNION DISTINCT count: {listDistinct.Count}");
        }
    }
}
