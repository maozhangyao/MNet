using MNet.SqlExpression;

IEnumerable<int> ids = new int[] { 1, 3, 5, 343 };


CEnumT c = new CEnumT();
c.Name = "hello";
c.Value = "sdfa";

var tem = from e in c.AsDbSet()
          where e.Id == 1001 && e.Name == "hello"
          orderby e.Id, e.Name
          select new { e.Id, e.Name };



public class CEnumT
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }

    public static CEnumT E1 = new CEnumT() { Value = "sss" };
    public static string Name1 = "abc";
}
