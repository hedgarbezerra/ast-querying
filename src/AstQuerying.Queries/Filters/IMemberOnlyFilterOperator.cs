using System.Linq.Expressions;

namespace AstQuerying.Queries.Filters;

/// <summary>
///     Filter operator that only inspects the member (for example <c>isNull</c>, <c>isNotNull</c>) with no separate
///     operand.
/// </summary>
public interface IMemberOnlyFilterOperator : IFilterOperator
{
    /// <summary>
    ///     Builds a boolean expression from <paramref name="memberAccess" /> alone.
    /// </summary>
    /// <param name="memberAccess">Expression that reads the target property or nested member.</param>
    /// <returns>An expression of type <see cref="bool" />.</returns>
    Expression CreateExpression(Expression memberAccess);
}