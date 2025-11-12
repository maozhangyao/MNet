using MNet.SqlExpression;

CEnumT c = new CEnumT();
c.Name = "hello";
c.Value = "sdfa";

var tem = from e in c.AsDbSet()
          where e.Id == 1001 && e.Name == "hello"
          orderby e.Id, e.Name descending
          select new { e.Id, e.Name };

string sql = tem.ToSql();
Console.WriteLine(sql);












public class CEnumT
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }

    public static CEnumT E1 = new CEnumT() { Value = "sss" };
    public static string Name1 = "abc";
}
