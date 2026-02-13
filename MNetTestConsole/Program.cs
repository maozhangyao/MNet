using MNet.LTSQL.v1;
using MNet.LTSQL.v1.SqlTokens;
using System.Reflection;
using System.Text;

c_persion_t c = new c_persion_t();
var query1 = from mine in c.AsLTSQL()
             join mother in c.AsLTSQL() on new { n = mine.MotherId } equals new { n = mother.Id }
             join father in c.AsLTSQL() on mine.FatherId equals father.Id
             where mine.Age > 18
             orderby mine.Age
             //select new { 
             //    Self = mine.SelfName,
             //    mine.Age, Mather = mother.SelfName,
             //    Father = father.SelfName, JoinName = string.Concat("mather: ", mother.SelfName, "father", father.SelfName) 
             //};
             select new info { Self = mine.SelfName, Mather = mother.SelfName, Father = father.SelfName };


var query2 = from e in c.AsLTSQL()
             where e.Id == 1
             select e;

LTSQLOptions options = new LTSQLOptions
{
    DbType = DbType.MySQL,
    UseSqlParameter = true
};

LTSQLToken token = new SequenceTranslater().Translate(query1.Query, options);
StringBuilder builder = new StringBuilder();
token.ToSql(new LTSQLTokenContext
{
    SQLBuilder = builder,
    Options = options
});
Console.WriteLine(builder);

return 0;

public class c_persion_t
{
    public int Id { get; set; }
    public int Age { get; set; }
    public string SelfName { get; set; }
    public int FatherId { get; set; }
    public int MotherId { get; set; }
}
public class info 
{
    public string Self { get; set; }
    public int? Age { get; set; }
    public string Mather { get; set; }
    public string Father { get; set; }
}

