using MNet.LTSQL;
using MNet.LTSQL.SqlTokens;
using MNetTestConsole.Utils;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations.Schema;
using MNet.LTSQL.Attributes;
using UnitTestModel;

CPersionT p = new CPersionT();

// 基础内连接查询 + 子查询 + 元组in匹配
var query1 = (from p1 in p.AsLTSQL()
              from p2 in p.AsLTSQL()
              from p3 in p.AsLTSQL()
              where !new { age = p1.Age, name = p1.SelfName }.In(p.AsLTSQL().Select(p => new { age = p.Age, name = p.SelfName }).Take(1))
              select new
              {
                  first = p1.Id,
                  second = p2.Id,
                  thrid = p3.Id,
                  dateTime = DateTime.Now.ToString("%Y %m-%H"),
                  year = DateTime.Now.Year,
                  month = DateTime.Now.Month,
                  day = DateTime.Now.Day,
                  hour = DateTime.Now.Hour,
                  minute = DateTime.Now.Minute,
                  second1 = DateTime.Now.Second
              });

Console.WriteLine("基础内连接查询 + 子查询 + 元组in匹配：");
ConsoleHelper.WriteLineWithYellow(query1.ToSql(DbType.SQLLite, out _, false));


// 右外联接+左外联接+多维度group by + having + order by
var query2 = from p1 in p.AsLTSQL().Where(p => p.Id > 1)
             join p2 in p.AsLTSQL().WithRight() on p1.MotherId equals p2.Id
             join p3 in p.AsLTSQL().WithLeft() on new { Id = p1.FatherId } equals new { Id = p3.Id }
             group new { Id1 = p1.Id, Id2 = p2.Id, Id3 = p3.Id } by new { Id1 = p1.Id, Id2 = p2.Id, Id3 = p3.Id } into gs
             where gs.Key.Id1 + gs.Key.Id2 + gs.Key.Id3 > 0
             orderby gs.Key.Id1 + gs.Key.Id3
             select new
             {
                 A = gs.Key.Id1,
                 Min = gs.Min(p => p.Id1),
                 Max = gs.Min(p => p.Id3)
             };

Console.WriteLine();
Console.WriteLine("右外联接+左外联接+多维度group by + having + order by：");
ConsoleHelper.WriteLineWithYellow(query2.ToSql(DbType.SQLLite, out _, false));


// 联合查询操作(union all)
var query3 = query1.UnionSet(query2, distinct: false);
Console.WriteLine();
Console.WriteLine("联合查询操作(union all)：");
ConsoleHelper.WriteLineWithYellow(query3.ToSql(DbType.SQLLite, out _, false));

return 0;
