using Dapper;
using Microsoft.Data.Sqlite;
using MNet.LTSQL;
using System.Data;
using UnitTestModel;
using Xunit;
using Xunit.Abstractions;

namespace LTSQLXUnitTest
{
    /// <summary>
    /// WHERE 条件查询测试类
    /// 测试各种 WHERE 条件的 SQL 生成和执行
    /// </summary>
    public class LTSQLWhereQueryTest
    {
        public LTSQLWhereQueryTest(ITestOutputHelper outp)
        {
            this._outp = outp;
        }

        private ITestOutputHelper _outp;

        /// <summary>
        /// 测试等于条件：WHERE Id = 5
        /// </summary>
        [Fact]
        public void Where_EqualCondition()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, var parameters) = persion.AsLTSQL()
                .Where(p => p.Id == 5)
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");
            if (parameters != null && parameters.Count > 0)
            {
                _outp.WriteLine($"Parameters: {string.Join(", ", parameters.Select(p => $"{p.key}={p.val}"))}");
            }

            List<CPersionT> list = connection.Query<CPersionT>(sql, parameters?.ToDictionary(p => p.key, p => p.val)).ToList();
            Assert.NotNull(list);
            
            foreach (var item in list)
            {
                Assert.Equal(5, item.Id);
                _outp.WriteLine($"{item.Id} - {item.SelfName}");
            }
        }

        /// <summary>
        /// 测试不等于条件：WHERE Id <> @p0
        /// </summary>
        [Fact]
        public void Where_NotEqualCondition()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Where(p => p.Id != 0)
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count > 0);

            foreach (var item in list)
            {
                Assert.NotEqual(0, item.Id);
            }
        }

        /// <summary>
        /// 测试大于条件：WHERE Age > @p0
        /// </summary>
        [Fact]
        public void Where_GreaterThanCondition()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Where(p => p.Age > 20)
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(item.Age > 20);
            }
        }

        /// <summary>
        /// 测试小于条件：WHERE Age < @p0
        /// </summary>
        [Fact]
        public void Where_LessThanCondition()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Where(p => p.Age < 50)
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(item.Age < 50);
            }
        }

        /// <summary>
        /// 测试大于等于条件：WHERE Age >= @p0
        /// </summary>
        [Fact]
        public void Where_GreaterThanOrEqualCondition()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Where(p => p.Age >= 25)
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(item.Age >= 25);
            }
        }

        /// <summary>
        /// 测试小于等于条件：WHERE Age <= @p0
        /// </summary>
        [Fact]
        public void Where_LessThanOrEqualCondition()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Where(p => p.Age <= 40)
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(item.Age <= 40);
            }
        }

        /// <summary>
        /// 测试 AND 多条件：WHERE Id > 0 AND Age < 50
        /// </summary>
        [Fact]
        public void Where_AndConditions()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Where(p => p.Id > 0 && p.Age < 50)
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(item.Id > 0 && item.Age < 50);
            }
        }

        /// <summary>
        /// 测试 OR 多条件：WHERE Id = 1 OR Id = 2
        /// </summary>
        [Fact]
        public void Where_OrConditions()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Where(p => p.Id == 1 || p.Id == 2)
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(item.Id == 1 || item.Id == 2);
            }
        }

        /// <summary>
        /// 测试组合条件：WHERE (Id > 0 AND Age < 50) OR SelfName LIKE '%test%'
        /// </summary>
        [Fact]
        public void Where_ComplexConditions()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Where(p => (p.Id > 0 && p.Age < 50) || p.SelfName.Contains("test"))
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);
        }

        /// <summary>
        /// 测试字符串包含：WHERE SelfName LIKE '%张%'
        /// </summary>
        [Fact]
        public void Where_StringContains()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Where(p => p.SelfName.Contains("张"))
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.Contains("张", item.SelfName);
            }
        }

        /// <summary>
        /// 测试字符串开头：WHERE SelfName LIKE '张%'
        /// </summary>
        [Fact]
        public void Where_StringStartsWith()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Where(p => p.SelfName.StartsWith("张"))
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.StartsWith("张", item.SelfName);
            }
        }

        /// <summary>
        /// 测试字符串结尾：WHERE SelfName LIKE '%三'
        /// </summary>
        [Fact]
        public void Where_StringEndsWith()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Where(p => p.SelfName.EndsWith("三"))
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.EndsWith("三", item.SelfName);
            }
        }

        /// <summary>
        /// 测试多个 Where 子句叠加
        /// </summary>
        [Fact]
        public void Where_MultipleWhereClauses()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Where(p => p.Id > 0)
                .Where(p => p.Age < 60)
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(item.Id > 0 && item.Age < 60);
            }
        }

        /// <summary>
        /// 测试 BETWEEN 范围查询（通过 >= 和 <= 组合）
        /// </summary>
        [Fact]
        public void Where_BetweenRange()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Where(p => p.Age >= 20 && p.Age <= 40)
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.True(item.Age >= 20 && item.Age <= 40);
            }
        }

        /// <summary>
        /// 测试 NOT 条件：WHERE NOT (Id = 0)
        /// </summary>
        [Fact]
        public void Where_NotCondition()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL()
                .Where(p => !(p.Id == 0))
                .ToSqlWithParameter(DbTypes.SQLLite, false);

            _outp.WriteLine($"SQL: {sql}");

            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            foreach (var item in list)
            {
                Assert.NotEqual(0, item.Id);
            }
        }
    }
}
