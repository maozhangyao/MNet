using MNet.LTSQL.v1;
using MNet.LTSQL.v1.SqlTokens;
using MNetTestConsole.Utils;
using System.Linq.Expressions;
using System.Text;

/*
TO DO
 1. having 的支持
 2. 分页的支持(Skip/Take扩展方法组合使用) [ok]
    2.1 fetch next 子句实现的分页
    2.2 limit 子句实现的分页
 3. 去重子句支持                          [ok]
 4. FirstOrDefault 支持

 笛卡尔积
 join into 句子 翻译
 对 is null / is not null 的支持

 5. 基础函数的支持
    5.1 字符串相关函数的支持
    5.2 日期函数的相关支持
 */


List<int> id1s = new List<int> { 1, 2, 3, 4 };
IEnumerable<int> id2s = id1s;

c_persion_t c = new c_persion_t();
var query1 = from mine in c.AsLTSQL()
             join mother in c.AsLTSQL() on mine.MotherId equals mother.Id
             join father in c.AsLTSQL() on mine.FatherId equals father.Id
             where (mine.Age > 0) && c.AsLTSQL().Select(p => p.Id).Any()
             group mine by new { Name = mine.SelfName, Age = mine.Age } into g
             select new
             {
                 NO = g.Key.Name,
                 SA = g.Sum(x => x.Age)
             };


var query2 = from e in c.AsLTSQL()
             where e.Id == 1
             group e by e.SelfName into g
             select new
             {
                 No = g.Key,
                 Cn = g.Sum(p => p.Id)
             };

LTSQLOptions options = new LTSQLOptions
{
    DbType = DbType.MySQL,
    UseSqlParameter = false, //是否参数化
};

//token 化
LTSQLToken token = new SequenceTranslater().Translate(query1.Distinct().Take(10).Skip(1).Query, options);

//生成的sql语句
StringBuilder builder = new StringBuilder();
//如果参数化，则SQL语句依赖的 sql 参数
Dictionary<string, object> sqlParamemters = new Dictionary<string, object>();


//sql化
ISqlBuilder sqlBuilder = LTSQLTokenSqlBuilder.Default;
sqlBuilder.Build(token, new SqlBuilderContext
{
    UseParameter = options.UseSqlParameter,
    DbType = options.DbType,
    Sql = builder,
    SqlParameters = sqlParamemters
});

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