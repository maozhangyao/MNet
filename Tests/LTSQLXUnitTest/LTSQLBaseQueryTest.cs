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
        public void Query002Where()
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
        public void Query003OrderBy()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();

            CPersionT persion = new CPersionT();
            (string sql, _) = persion.AsLTSQL()
                .OrderByDescending(p => p.Id)
                .ToSql(MNet.LTSQL.DbType.SQLLite, false);

            Assert.NotNull(sql);
            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);

            Assert.True(list.Count > 1, sql);
            bool flag = true;
            CPersionT pre = list[0];
            foreach (var item in list.Skip(1))
            {
                flag &= pre.Id >= item.Id;
                pre = item;
            }

            Assert.True(flag, sql);
        }

        //基础查询 group by
        [Fact]
        public void Query004GroupBy()
        {
            CPersionT persion = new CPersionT();
            using IDbConnection connection = DbConnectionFactory.Sqllite();

            (string sql, _) = (
                    from p in persion.AsLTSQL()
                    group p by p.SelfName into g
                    select new CPersionSelect1
                    {
                        Name = g.Key
                    }
                ).ToSql(MNet.LTSQL.DbType.SQLLite, false);

            this._outp.WriteLine(sql);

            List<CPersionSelect1> list = connection.Query<CPersionSelect1>(sql).ToList();
            Assert.True(list.Count > 0);

            foreach (var item in list)
            {
                Assert.NotNull(item.Name);
                this._outp.WriteLine(item.Name);
            }
        }

        //基础查询 group by having
        [Fact]
        public void Query005GroupByHaving()
        {
            CPersionT persion = new CPersionT();
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            (string sql, _) = (
                    from p in persion.AsLTSQL()
                    group p by p.SelfName into g
                    where g.Key.Length > 3
                    select new CPersionSelect1
                    {
                        Name = g.Key
                    }
                ).ToSql(MNet.LTSQL.DbType.SQLLite, false);

            this._outp.WriteLine(sql);
            List<CPersionSelect1> list = connection.Query<CPersionSelect1>(sql).ToList();
            Assert.True(list.Count > 0);
            foreach (var item in list)
            {
                Assert.NotNull(item.Name);
                this._outp.WriteLine(item.Name);
            }
        }

        //基础查询 select
        [Fact]
        public void Query006Select()
        {
            CPersionT persion = new CPersionT();
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            (string sql, _) = (
                    from p in persion.AsLTSQL()
                    select new CPersionSelect1
                    {
                        Id = p.Id,
                        Name = p.SelfName
                    }
                ).ToSql(MNet.LTSQL.DbType.SQLLite, false);

            this._outp.WriteLine(sql);
            List<CPersionSelect1> list = connection.Query<CPersionSelect1>(sql).ToList();
            Assert.True(list.Count > 0);
            foreach (var item in list)
            {
                Assert.NotNull(item.Name);
                this._outp.WriteLine($"{item.Id} - {item.Name}");
            }
        }


    }
}

