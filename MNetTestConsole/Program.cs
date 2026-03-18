using MNet.LTSQL.v1;
using MNet.LTSQL.v1.SqlTokens;
using MNetTestConsole.Utils;
using System.Numerics;
using System.Reflection;
using System.Text;

/*
TO DO
 QuerySequence 是不可变类型，才能复用逻辑


 笛卡尔积
 join into 句子 翻译
 对 is null / is not null 的支持

 5. 基础函数的支持
    5.1 字符串相关函数的支持
    5.2 日期函数的相关支持

优化：
 ConstantToken 类设计优化，硬编码和SQL值分离
 检查对主流数据库的支持
 ConditionToken.Not操作不准确：比如AND , OR 等操作取反不对
 */


List<int> id1s = new List<int> { 1, 2, 3, 4 };
IEnumerable<int> id2s = id1s;

c_persion_t c = new c_persion_t();
var query1 = from mine in c.AsLTSQL()
             join mother in c.AsLTSQL() on mine.MotherId equals mother.Id
             join father in c.AsLTSQL() on mine.FatherId equals father.Id
             where (mine.Age > 0)
             group mine by new { Name = mine.SelfName, Age = mine.Age } into g
             select new
             {
                 NO = g.Key.Name,
                 SA = g.Sum(x => x.Age),  //求和
                 AV = g.Average(x => x.Age), //求平均值
                 MX = g.Max(x => x.Age),  //求最大值
                 MN = g.Min(x => x.Age),  //求最小值
                 CN = g.Count()          //求计数
             };


var query2 = from e in c.AsLTSQL()
             where e.Id == 1
             group e by e.SelfName into g
             select new
             {
                 No = g.Key,
                 Cn = g.Sum(p => p.Id)
             };

var query3 = c.AsLTSQL();


LTSQLOptions options = new LTSQLOptions
{
    DbType = DbType.SQLLite,
    UseSqlParameter = false, //是否参数化
};

//token 化
LTSQLToken token = new SequenceTranslater().Translate(query1.Query, options);


//生成的sql语句
StringBuilder builder = new StringBuilder();
//如果参数化，则SQL语句依赖的 sql 参数
Dictionary<string, object> sqlParamemters = new Dictionary<string, object>();


//sql化
ISqlBuilder sqlBuilder = LTSQLTokenSqlBuilder.Default;
sqlBuilder.Build(token, new SqlBuilderContext
{
    DbType = options.DbType,
    UseParameter = options.UseSqlParameter,
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