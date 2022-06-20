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
