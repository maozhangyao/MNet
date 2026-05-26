using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using MNet.LTSQL;
using System.Collections;
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
        public void Query001SimplySelect()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();

            CPersionT persion = new CPersionT();

            (string sql, _) = persion.AsLTSQL().ToSql(MNet.LTSQL.DbTypes.SQLLite, false);

            this._outp.WriteLine(sql);
            List<CPersionT> list = connection.Query<CPersionT>(sql).ToList();
            Assert.NotNull(list);
            Assert.True(list.Count > 0, sql);

            this._outp.WriteLine("");
            foreach (CPersionT item in list)
            {
                this._outp.WriteLine($"{item.Id} - {item.SelfName}");
                Assert.NotNull(item.SelfName);
            }
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
                .ToSql(MNet.LTSQL.DbTypes.SQLLite, false);

            this._outp.WriteLine(sql);

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
                .ToSql(MNet.LTSQL.DbTypes.SQLLite, false);

            this._outp.WriteLine(sql);

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
                ).ToSql(MNet.LTSQL.DbTypes.SQLLite, false);

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
                ).ToSql(MNet.LTSQL.DbTypes.SQLLite, false);

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
                ).ToSql(MNet.LTSQL.DbTypes.SQLLite, false);

            this._outp.WriteLine(sql);
            List<CPersionSelect1> list = connection.Query<CPersionSelect1>(sql).ToList();
            Assert.True(list.Count > 0);
            foreach (var item in list)
            {
                Assert.NotNull(item.Name);
                this._outp.WriteLine($"{item.Id} - {item.Name}");
            }
        }

        //基础查询 inner join（默认）
        [Fact]
        public void Query007InnerJoin()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();
            CCourseT course = new CCourseT();

            {
                this._outp.WriteLine("默认带条件的 inner join:");
                string sql = (from p in persion.AsLTSQL()
                              from t in teacher.AsLTSQL()
                              from c in course.AsLTSQL()
                              where p.Id == t.PersionId && t.CourseId == c.Id
                              select new CPersionSelect1
                              {
                                  Id = p.Id,
                                  Name = string.Concat(string.Concat(p.SelfName, "-"), c.Course)
                              }).ToSql(DbTypes.SQLLite, out _, false);

                var list = connection.Query<CPersionSelect1>(sql).ToList();
                this._outp.WriteLine(sql);
                Assert.NotEmpty(list);
                foreach (var item in list)
                {
                    Assert.NotNull(item.Name);
                    this._outp.WriteLine($"{item.Id} - {item.Name}");
                }
            }

            {
                this._outp.WriteLine("默认不带条件的 inner join:");
                string sql = (from p in persion.AsLTSQL()
                              from t in teacher.AsLTSQL()
                              from c in course.AsLTSQL()
                              select new CPersionSelect1
                              {
                                  Id = p.Id,
                                  Name = string.Concat(string.Concat(p.SelfName, "-"), c.Course)
                              }).ToSql(DbTypes.SQLLite, out _, false);

                var list = connection.Query<CPersionSelect1>(sql).ToList();
                this._outp.WriteLine(sql);
                Assert.NotEmpty(list);
                foreach (var item in list)
                {
                    Assert.NotNull(item.Name);
                    this._outp.WriteLine($"{item.Id} - {item.Name}");
                }
            }
        }

        //基础查询 inner join（显示）
        [Fact]
        public void Query008InnerJoin()
        {
            using IDbConnection connection = DbConnectionFactory.Sqllite();
            CPersionT persion = new CPersionT();
            CTeacherT teacher = new CTeacherT();
            CCourseT course = new CCourseT();

            {
                this._outp.WriteLine("不带条件的 inner join:");
                string sql = (from p in persion.AsLTSQL()
                              join t in teacher.AsLTSQL().WithInner() on p.Id equals t.PersionId
                              join c in course.AsLTSQL().WithInner() on t.CourseId equals c.Id
                              select new CPersionSelect1
                              {
                                  Id = p.Id,
                                  Name = string.Concat(string.Concat(p.SelfName, "-"), c.Course)
                              }).ToSql(DbTypes.SQLLite, out _, false);

                var list = connection.Query<CPersionSelect1>(sql).ToList();
                this._outp.WriteLine(sql);
                Assert.NotEmpty(list);
                foreach (var item in list)
                {
                    Assert.NotNull(item.Name);
                    this._outp.WriteLine($"{item.Id} - {item.Name}");
                }
            }

            {
                this._outp.WriteLine("带条件的 inner join:");
                string sql = (from p in persion.AsLTSQL()
                              join t in teacher.AsLTSQL().WithInner() on p.Id equals t.PersionId
                              join c in course.AsLTSQL().WithInner() on t.CourseId equals c.Id
                              where p.Id < 100
                              select new CPersionSelect1
                              {
                                  Id = p.Id,
                                  Name = string.Concat(string.Concat(p.SelfName, "-"), c.Course)
                              }).ToSql(DbTypes.SQLLite, out _, false);

                var list = connection.Query<CPersionSelect1>(sql).ToList();
                this._outp.WriteLine(sql);
                Assert.NotEmpty(list);
                foreach (var item in list)
                {
                    Assert.NotNull(item.Name);
                    this._outp.WriteLine($"{item.Id} - {item.Name}");
                }
            }
        }
    }
}

