using MNet.SqlExpression;
using System.Data.Common;

C_Enum_t c = new C_Enum_t();
c.Display = "hello";
c.EnumValue = "sdfa";

var tem = from p in c.AsDbSet()
          where (from e1 in c.AsDbSet() where e1.EnumCode == p.EnumCode + 1 select e1.EnumCode).First() == p.EnumCode
          select p;

//var tem = from e1 in c.AsDbSet() where e1.EnumCode == 1 select e1.EnumCode;

string sql = tem.ToSql();
Console.WriteLine(sql);




public class C_Enum_t
{
    public int EnumCode { get; set; }
    public string Display { get; set; }
    public string EnumValue { get; set; }

    public static C_Enum_t E1 = new C_Enum_t() { EnumValue = "sss" };
    public static string Name1 = "abc";
}
