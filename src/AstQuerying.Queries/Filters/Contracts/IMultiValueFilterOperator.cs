using System.Linq.Expressions;

namespace AstQuerying.Queries.Filters.Contracts;

/// <summary>
/// Filter operator that compares a member against a list of operand expressions (for example <c>in</c>, <c>notIn</c>).
/// </summary>
public interface IMultiValueFilterOperator : IFilterOperator
{
    /// <summary>
    /// Builds a boolean expression for set-style comparison of <paramref name="memberAccess"/> against <paramref name="values"/>.
    /// </summary>
    /// <param name="memberAccess">Expression that reads the target property or nested member.</param>
    /// <param name="values">One or more right-hand operands.</param>
    /// <returns>An expression of type <see cref="bool"/>.</returns>
    Expression CreateExpression(Expression memberAccess, IReadOnlyList<Expression> values);
}
