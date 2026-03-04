using MNet.LTSQL.v1;
using MNet.LTSQL.v1.SqlTokens;
using MNetTestConsole.Utils;
using System.Linq.Expressions;
using System.Text;

/*
 1. group by 处理 已解决
 2. 子查询编译支持
 3. IN 操作的支持
    3.1 list token 的表达
 4. NOT IN 操作的支持
    TokenItemListToken => a,b,c,d,e

 5. 如何编译，可能会影响到 list 拆包

 */


List<int> id1s = new List<int> { 1, 2, 3, 4 };
IEnumerable<int> id2s = id1s;

c_persion_t c = new c_persion_t();
var query1 = from mine in c.AsLTSQL()
             join mother in c.AsLTSQL() on mine.MotherId equals mother.Id
             join father in c.AsLTSQL() on mine.FatherId equals father.Id
             where (mine.Age > 0) && c.AsLTSQL().Any()
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
    UseSqlParameter = true, //是否参数化
};

//token 化
LTSQLToken token = new SequenceTranslater().Translate(query2.Query, options);


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