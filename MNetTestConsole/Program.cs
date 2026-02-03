using MNet.SqlExpression;
using System.Data.Common;
using MNet.LTSQL.v1;

C_Enum_t c = new C_Enum_t();
var query = from e in c.AsLTSQL()
            join e2 in c.AsLTSQL() on e.EnumCode equals e2.EnumCode
            where e.EnumCode == 1 && e.Display.Contains("hello")
            select new { Code = e.EnumCode };


Console.WriteLine(query);

return 0;




public class C_Enum_t
{
    public int EnumCode { get; set; }
    public string Display { get; set; }
    public string EnumValue { get; set; }

    public static C_Enum_t E1 = new C_Enum_t() { EnumValue = "sss" };
    public static string Name1 = "abc";
}
