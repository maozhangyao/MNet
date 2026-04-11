using MNet.LTSQL.v1;
using MNet.LTSQL.v1.SqlTokens;
using MNetTestConsole.Utils;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;

/*
 基础函数的支持                    [ok]
    5.1 字符串相关函数的支持       [ok]
    5.2 日期函数的相关支持         [ok

优化：
ConstantToken 类设计优化，硬编码和SQL值分离， 增加文本Token来区分   [ok]
SqlScopeToken 需要拆分出优先级运算token：PriorityCalcToken          [ok]
ConditionToken.Not操作不准确：比如不应该支持AND , OR 等操作取反     [ok]
在翻译条件表达式树过程中， 关系比较(> = <)和算数运算中(+ - * /)，对可空类型和非可空类型的处理不够友好，比如：不支持 int == int? ， 增加对可空类型的支持  [ok]
SyntaxToken 增加 关键字转移开关：实现统一的关键字转义 [ok]

在 where a.field == ().firstOrdefault() 场景如何解决
boolCaclToken 与 binaryToken 做部分合并，boolCaclToken中只能包含bool操作， < > >= <= != 等操作不属于bool操作

SequenceToken
 orderby 和 goupby 执行顺序问题
 检查对主流数据库的支持情况

 接收参数的Dictionary容器替换掉，避免大量参数生成的情况
 表名，字段名的自定义映射
 */
string n = null;

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
             where p1.Id > 0
             select new { Id = p1.Id, Name = p1.SelfName, MName = p2.SelfName, FName = p3.SelfName };


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