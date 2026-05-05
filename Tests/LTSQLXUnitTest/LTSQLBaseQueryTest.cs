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
    /// 基础查询测试类
    /// </summary>
    public class LTSQLBaseQueryTest
    {
        public LTSQLBaseQueryTest(ITestOutputHelper outp)
        {
            this._outp = outp;
        }

        private ITestOutputHelper _outp;


        /// <summary>
        /// 基础查询：select * from xxx
        /// </summary>
        [Fact]
        public void Query001()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();

            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL().ToSql(MNet.LTSQL.DbType.SQLLite, false);

            Assert.NotNull(sql);
            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count > 0, sql);
        }

        /// <summary>
        /// 基础查询：select * from xxx where xxx
        /// </summary>
        [Fact]
        public void Query002()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();

            CPersionT persion = new CPersionT();
            (string sql, _) = persion.AsLTSQL()
                .Where(p => p.Id == 0)
                .ToSql(MNet.LTSQL.DbType.SQLLite, false);

            Assert.NotNull(sql);
            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count == 1, sql);
        }

        /// <summary>
        /// 基础查询, 根据id排序：select * from xxx order by xxx
        /// </summary>
        [Fact]
        public void Query003_OrderBy()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();

            CPersionT persion = new CPersionT();
            (string sql, _) = persion.AsLTSQL()
                .OrderByDescending(p => p.Id)
                .ToSql(MNet.LTSQL.DbType.SQLLite, false);

            Assert.NotNull(sql);
            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            Assert.True(list.Count > 1 , sql);
            bool flag = true;
            CPersionT pre = list[0];
            foreach(var item in list.Skip(1))
            {
                flag &= pre.Id >= item.Id;
                pre = item;
            }

            Assert.True(flag, sql);
        }
    }

}

