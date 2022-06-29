using System.Collections.Generic;
using System.Reflection;

using Microsoft.Data.Sqlite;

static class _Deserialize
{
	public static List<T> Deserialize<T, TRes>(StateMen<T, TRes> statement, SqliteConnection con) where T : new()
	{
		SqliteCommand command = new SqliteCommand(statement.ToSql(), con);

		var reader = command.ExecuteReader();

		var props = typeof(TRes).GetProperties();

		List<T> res = new();
		while(reader.Read())
		{
			T resRow = new();

			foreach(var prop in props)
			{
				typeof(T).GetProperty(prop.Name)?.SetValue(resRow, Convert.ChangeType(reader[prop.Name], prop.PropertyType));
				typeof(T).GetField(prop.Name)?.SetValue(resRow, Convert.ChangeType(reader[prop.Name], prop.PropertyType));
			}

			res.Add(resRow);
		}

		return res;
	}
}
