using MNet.LTSQL;
using MNet.LTSQL.SqlTokens;
using MNetTestConsole.Utils;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations.Schema;
using MNet.LTSQL.Attributes;

/*

 检查对主流数据库的支持情况                      

 */

var arr = new List<int>() { 1, 2, 3 };
var arr1 = new[] { new { age = 1, name = "ymz" }, new { age = 35, name = "金刚" } };


c_persion_t p = new c_persion_t();
p.Id = 1000;

var query1 = (from p1 in p.AsLTSQL()
              from p2 in p.AsLTSQL()
              from p3 in p.AsLTSQL()
              where 
              !new { age = p1.Age, name = p1.SelfName }.In(p.AsLTSQL().Select(p => new { age = p.Age, name = p.SelfName ?? "NUA" }).Take(1))
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

var query3 = query1.UnionSet(query2);


try
{
    (string sql, var parameters) = query3.ToSql(DbType.SQLLite, false);
    ConsoleHelper.WriteLineWithYellow(sql);
}
catch (Exception ex)
{
    ConsoleHelper.WriteLineWithRed(ex);
    throw;
}

return 0;


[Table("c_persion_t")]
public class c_persion_t
{
    [Column("id")]
    public int Id { get; set; }

    //[NonFiled]
    public int Age { get; set; }

    public string SelfName { get; set; }
    //[Column("FId")]
    public int FatherId { get; set; }

    //[Column("MId")]
    public int MotherId { get; set; }
}