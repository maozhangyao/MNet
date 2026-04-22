using MNet.LTSQL;
using MNet.LTSQL.SqlTokens;
using MNetTestConsole.Utils;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;

/*
 实现sql格式化输出接口，屏蔽直接对sql字符串的拼接                   [ok] 
 接收参数的Dictionary容器替换掉，避免大量参数生成的情况             [ok]
 IN 操作考虑支持元组匹配                                          [ok]
 
 select 独立语句支持：如 select 1， 无需from子句                   [ok]
 select union all 支持
 withAny逻辑优化，直接selet 1 或者 select 0
 ?? 运算符支持
 子查询的作用域范围优化，对于是否需要包裹括号, 进一步判断
 表名，字段名的自定义映射

 检查对主流数据库的支持情况                      

 */

var arr = new List<int>() { 1, 2, 3 };
var arr1 = new[] { new { age = 1, name = "ymz" }, new { age = 35, name = "金刚" } };


c_persion_t p = new c_persion_t();
p.Id = 1000;

var query1 = (from p1 in p.AsLTSQL()
              from p2 in p.AsLTSQL()
              from p3 in p.AsLTSQL()
              where new { age = p1.Age, name = p1.SelfName }.In(p.AsLTSQL().Select(p => new { age = p.Age, name = p.SelfName }).Take(1))
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
                 Min = gs.Min(p => p.Id1),
                 Max = gs.Min(p => p.Id3)
             };

var query3 = p.AsSelect(p => new {age = p.Age, name = p.SelfName});

LTSQLOptions options = new LTSQLOptions
{
    DbType = DbType.SQLLite,
    UseSqlParameter = false, //是否参数化
    DisNullable = false
};

//token 化
LTSQLToken token = new SequenceTranslater().Translate(query3.Query, options);

try
{
    //sql化
    ISqlBuilder sqlBuilder = LTSQLTokenSqlBuilder.Default;
    SqlBuilderContext ctx = new SqlBuilderContext();
    ctx.DbType = options.DbType;
    ctx.UseParameter = options.UseSqlParameter;
    ctx.SqlWriterFactory = () => new LTSQLWriter(true);
    sqlBuilder.Build(token, ctx);

    ConsoleHelper.WriteLineWithYellow(ctx.Sql);
}
catch (Exception ex)
{
    ConsoleHelper.WriteLineWithRed(ex);
    throw;
}

return 0;


public class c_persion_t
{
    public int Id { get; set; }
    public int Age { get; set; }

    public string SelfName { get; set; }
    public int FatherId { get; set; }
    public int MotherId { get; set; }
}