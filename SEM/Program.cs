StateMen<X> x = new();

Console.WriteLine(x.Select(a => new { a.a, a.b })
		.Where(a => a.b >= 5)
		.Where(a => a.a < 12)
		.OrderBy(a => a.a, true)
		.OrderBy(a => a.b)
		.GroupBy(a => a.a)
.ToSql());

class X
{
	public int a;
	public int b;

	public int HELLO;
}
