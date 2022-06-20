using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// fancy men from upstate texas commonwealth.
/// </summary>
public class StateMen<T>
{
	private List<INode> statements = new();

	public StateMen<TRes> Select<TRes>(Expression<Func<T, TRes>> expression)
	{
		if (statements.Any())
			throw new InvalidOperationException("Select must be the first StateMen");
		SelectNode res = new()
		{
			ToSelect = expression.Body switch
			{
				NewExpression nE => nE.Arguments.Where(x => x is MemberExpression { NodeType: ExpressionType.MemberAccess })
					.Cast<MemberExpression>()
					.Select(x => x.Member.Name).ToList(),
				MemberExpression { NodeType: ExpressionType.MemberAccess } mE => new List<string>() { mE.Member.Name },
				ParameterExpression pE => new List<string>() { "*" },
				_ => throw new NotSupportedException(),
			},
			From = typeof(T).Name,
		};
		return new StateMen<TRes>()
		{
			statements = {
				res
			}
		};
	}

	public StateMen<T> Where(Expression<Func<T, bool>> expression)
	{
		return this;
	}

	public string ToSql()
	{
		return string.Join(' ', statements.Select(x => x.ToSql()));
	}
}
