using System.Data.Common;
using Microsoft.Data.Sqlite;

using static _Deserialize;

using var db = new SqliteConnection(new SqliteConnectionStringBuilder()
{
	DataSource = "db.db"
}.ConnectionString);

db.Open();


var x = new StateMen<X>()
		.Select(a => new { a.a, a.HELLO})
		.Where(a => a.b < 31)
		.Where(a => a.a > 12)
		.OrderBy(a => a.a, true)
		.GroupBy(a => a.a);

Deserialize(x, db);

class X
{
	public int a;
	public int b;

	public string HELLO;
}
