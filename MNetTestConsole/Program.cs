using MNet.LTSQL;
using MNet.LTSQL.SqlTokens;
using MNetTestConsole.Utils;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;

/*
 实现sql格式化输出接口，屏蔽直接对sql字符串的拼接
 接收参数的Dictionary容器替换掉，避免大量参数生成的情况
 表名，字段名的自定义映射

 检查对主流数据库的支持情况                      

 */



string n = null;
int i = 0;
c_persion_t p = new c_persion_t();
var query1 = from p1 in p.AsLTSQL().Where(p => p.Id > 1)
             from p2 in p.AsLTSQL()
             from p3 in p.AsLTSQL()
             where !(p1.SelfName.Contains("女")) && p1.Id == (p.AsLTSQL().Select(p => p.Id).FirstOrDefault())
             select new { first = p1.Id, second = p2.Id, thrid = p3.Id,
                 dateTime = DateTime.Now.ToString("%Y %m-%H"),
                 year = DateTime.Now.Year,
                 month = DateTime.Now.Month,
                 day = DateTime.Now.Day,
                 hour = DateTime.Now.Hour,
                 minute = DateTime.Now.Minute,
                 second1 = DateTime.Now.Second
             };

var query2 = from p1 in p.AsLTSQL().Where(p => p.Id > 1)
             join p2 in p.AsLTSQL().WithRight() on p1.MotherId equals p2.Id
             join p3 in p.AsLTSQL().WithLeft() on new { Id = p1.FatherId } equals new { Id = p3.Id }
             group new {Id1 = p1.Id, Id2 = p2.Id, Id3 = p3.Id} by new { Id1 = p1.Id, Id2 = p2.Id, Id3 = p3.Id } into gs
             where gs.Key.Id1 + gs.Key.Id2 + gs.Key.Id3 > 0
             orderby gs.Key.Id1 + gs.Key.Id3
             select new {
                 Min = gs.Min(p => p.Id1),
                 Max = gs.Min(p => p.Id3)
             };


LTSQLOptions options = new LTSQLOptions
{
    DbType = DbType.SQLLite,
    UseSqlParameter = false, //是否参数化
    DisNullable = false
};

//token 化
LTSQLToken token = new SequenceTranslater().Translate(query1.Query, options);


//生成的sql语句
StringBuilder builder = new StringBuilder();
//如果参数化，则SQL语句依赖的 sql 参数
Dictionary<string, object> sqlParamemters = new Dictionary<string, object>();

try
{

    //sql化
    ISqlBuilder sqlBuilder = LTSQLTokenSqlBuilder.Default;
    sqlBuilder.Build(token, new SqlBuilderContext
    {
        DbType = options.DbType,
        UseParameter = options.UseSqlParameter,
        Sql = builder,
        SqlParameters = sqlParamemters
    });

}
catch (Exception ex)
{

    throw;
}
ConsoleHelper.WriteLineWithYellow(builder);


return 0;


public class c_persion_t
{
    public int Id { get; set; }
    public int Age { get; set; }

    public string SelfName { get; set; }
    public int FatherId { get; set; }
    public int MotherId { get; set; }
}