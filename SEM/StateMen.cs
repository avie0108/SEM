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
		if (!statements.Any())
			throw new InvalidOperationException("Where can not be the first StateMen");


		string clause = expression.Body.ToString()
			.Replace(expression.Parameters[0].Name + ".", "")
			.Replace("AndAlso", "&&")
			.Replace("OrElse", "||");

		
		statements.Add(new WhereNode {Clause = clause});

		return this;
	}

	public StateMen<T> OrderBy<TRes>(Expression<Func<T, TRes>> expression, bool descending = false)
	{
		if (!statements.Any())
			throw new InvalidOperationException("OrderBy cannot be the first StateMen");
		OrderByNode res = new()
		{
			ToOrder = expression.Body switch
			{
				NewExpression nE => nE.Arguments.Where(x => x is MemberExpression { NodeType: ExpressionType.MemberAccess })
					.Cast<MemberExpression>()
					.Select(x => x.Member.Name).ToList(),
				MemberExpression { NodeType: ExpressionType.MemberAccess } mE => new List<string>() { mE.Member.Name },
				_ => throw new NotSupportedException(),
			},
			Descending = descending,
		};

		statements.Add(res);
		return this;
	}

	public StateMen<T> GroupBy<TRes>(Expression<Func<T, TRes>> expression)
	{
		if (!statements.Any())
			throw new InvalidOperationException("OrderBy cannot be the first StateMen");
		GroupByNode res = new()
		{
			ToGroup = expression.Body switch
			{
				NewExpression nE => nE.Arguments.Where(x => x is MemberExpression { NodeType: ExpressionType.MemberAccess })
					.Cast<MemberExpression>()
					.Select(x => x.Member.Name).ToList(),
				MemberExpression { NodeType: ExpressionType.MemberAccess } mE => new List<string>() { mE.Member.Name },
				_ => throw new NotSupportedException(),
			},
		};

		statements.Add(res);
		return this;
	}

	public string ToSql()
	{
		List<string> Query = new();

		Query.Add(statements.Where(x => x is SelectNode).First().ToSql());

		try {
			Query.Add(statements.Where(x => x is WhereNode).Cast<WhereNode>().Aggregate((x, y) => new () {
				Clause = x.Clause + " && " + y.Clause,
			}).ToSql());
		}
		// if there are no Where StateMen it will throw this exception and we will just ignore it
		catch (InvalidOperationException) {}

		{
			var nodes = statements.Where(x => x is GroupByNode).Cast<GroupByNode>();
			if (nodes.Any())
			{
				string groupby = nodes.Aggregate("GROUP BY ", (acc, node) =>
					acc + string.Join(", ", node.ToGroup) + ", "
				);
				// remove the last ", "
				Query.Add(groupby.Remove(groupby.Length - 2));
			}
		}

		{
			var nodes = statements.Where(x => x is OrderByNode).Cast<OrderByNode>();
			if (nodes.Any())
			{
				string orderby = nodes.Aggregate("ORDER BY ", (acc, node) =>
					acc + string.Join(", ", node.ToOrder.Select(x => x + (node.Descending ? " DESC" : " ASC"))) + ", "
				);
				// remove the last ", "
				Query.Add(orderby.Remove(orderby.Length - 2));
			}
		}

		return string.Join('\n', Query);
	}
}
