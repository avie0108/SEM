public interface INode
{
	string ToSql();
}

public class SelectNode : INode
{
	public List<string> ToSelect { get; init; }
	public string From = "";

	public string ToSql()
	{
		return $"SELECT {string.Join(", ", ToSelect)} FROM {From}";
	}
}

public class WhereNode : INode
{
	public string Clause = "";

	public string ToSql()
	{
		return "WHERE (" + Clause + ')';
	}
}

public class OrderByNode : INode
{
	public List<string> ToOrder { get; init; }
	public bool Descending = false;

	public string ToSql()
	{
		return $"ORDER BY ({string.Join(", ", ToOrder.Select(x => x + (Descending ? " DESC" : " ASC")))})";
	}
}

public class GroupByNode : INode
{
	public List<string> ToGroup { get; init; }


	public string ToSql()
	{
		return $"GROUP BY ({string.Join(", ", ToGroup)})";
	}
}