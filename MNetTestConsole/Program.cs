using MNet.SqlExpression;
using System.Data.Common;

CEnumT c = new CEnumT();
c.Name = "hello";
c.Value = "sdfa";

var tem = from e in c.AsDbSet()
          where e.Id == (from e1 in c.AsDbSet() where e1.Id == e.Id + 1 select e1.Id).First()
          select e;


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
