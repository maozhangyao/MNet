using MNet.LTSQL.v1;
using MNet.SqlExpression;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.ObjectiveC;
using System.Runtime.Loader;

C_Enum_t c = new C_Enum_t();
var query1 = from e in c.AsLTSQL()
             join e2 in c.AsLTSQL() on e.EnumCode equals e2.EnumCode
             join e3 in c.AsLTSQL() on e.EnumCode equals e3.EnumCode
             where c.AsLTSQL().Where(p => p.EnumCode == e.EnumCode).Any()
             select new { Code = e.EnumCode };


var query2 = from e in c.AsLTSQL()
             where e.EnumCode == 1
             select e;

new SequenceTranslater().Translate(query1.Query, null);
return 0;




public class C_Enum_t
{
    public int EnumCode { get; set; }
    public string Display { get; set; }
    public string EnumValue { get; set; }

    public static C_Enum_t E1 = new C_Enum_t() { EnumValue = "sss" };
    public static string Name1 = "abc";
}
