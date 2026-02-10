using MNet.LTSQL.v1;
using MNet.SqlExpression;
using System.Linq.Expressions;

//C_Enum_t c = new C_Enum_t();
//var query1 = from e in c.AsLTSQL()
//             join e2 in c.AsLTSQL() on new { n = e.EnumValue } equals new { n = "" }
//             join e3 in c.AsLTSQL() on e.EnumCode equals e3.EnumCode
//             where c.AsLTSQL().Where(p => p.EnumCode == e.EnumCode).Any() && e.Parent == e2.Parent
//             select new { Code = e.EnumCode };


//var query2 = from e in c.AsLTSQL()
//             where e.EnumCode == 1
//             select e;

//new SequenceTranslater().Translate(query1.Query, null);

LambdaExpression expr = () => new C_Enum_t() { EnumCode = 1 };
MemberInitExpression body = expr.Body as MemberInitExpression;

Console.WriteLine(expr);



return 0;




public class C_Enum_t
{
    public int EnumCode { get; set; }
    public string Display { get; set; }
    public string EnumValue { get; set; }
    public C_Enum_t Parent { get; set; }
}
