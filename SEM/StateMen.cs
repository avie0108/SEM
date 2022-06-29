using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

// only used to start the expression
public class StateMen<T>
{
	public StateMen<T, _TRes> Select<_TRes>(Expression<Func<T, _TRes>> expression)
	{
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
		return new StateMen<T, _TRes>()
		{
			statements = {
				res
			}
		};
	}
}

/// <summary>
/// fancy men from upstate texas commonwealth.
/// </summary>
public class StateMen<T, TRes>
{
	public List<INode> statements = new();

	public StateMen<T, TRes> Where(Expression<Func<T, bool>> expression)
	{
		if (!statements.Any())
			throw new InvalidOperationException("Where can not be the first StateMen");


		string clause = expression.Body.ToString()
			.Replace(expression.Parameters[0].Name + ".", "")
			.Replace("AndAlso", "AND")
			.Replace("OrElse", "OR");

		
		statements.Add(new WhereNode {Clause = clause});

		return this;
	}

	public StateMen<T, TRes> OrderBy<_TRes>(Expression<Func<TRes, _TRes>> expression, bool descending = false)
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

	public StateMen<T, TRes> GroupBy<_TRes>(Expression<Func<TRes, _TRes>> expression)
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
				Clause = x.Clause + " AND " + y.Clause,
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
