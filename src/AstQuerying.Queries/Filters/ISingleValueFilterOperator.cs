using System.Linq.Expressions;

namespace AstQuerying.Queries.Filters;

/// <summary>
///     Filter operator that compares a member to exactly one operand expression (for example <c>=</c>, <c>&gt;</c>,
///     <c>contains</c>).
/// </summary>
public interface ISingleValueFilterOperator : IFilterOperator
{
    /// <summary>
    ///     Builds a boolean expression comparing <paramref name="memberAccess" /> to <paramref name="value" />.
    /// </summary>
    /// <param name="memberAccess">Expression that reads the target property or nested member.</param>
    /// <param name="value">The right-hand operand (already a constant or coerced expression).</param>
    /// <returns>An expression of type <see cref="bool" />.</returns>
    Expression CreateExpression(Expression memberAccess, Expression value);
}