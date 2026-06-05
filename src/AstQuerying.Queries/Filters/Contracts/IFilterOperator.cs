using System.Linq.Expressions;

namespace AstQuerying.Queries.Filters.Contracts;

/// <summary>
/// Defines a named filter operator that can translate a comparison into a LINQ predicate fragment
/// against a member access expression.
/// </summary>
/// <remarks>
/// Operators are keyed by <see cref="Name"/> (case-insensitive) in <see cref="T:AstQuerying.Queries.Registry.Contracts.IQueryRegistry.Operators"/>.
/// </remarks>
public interface IFilterOperator
{
    /// <summary>
    /// Gets the operator key used in API payloads (for example <c>=</c>, <c>in</c>, <c>contains</c>).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Builds a boolean expression for the given member access and optional operand expressions.
    /// </summary>
    /// <param name="memberAccess">Expression that reads the target property or nested member.</param>
    /// <param name="declaredLeafType">The CLR type declared for the leaf property in metadata.</param>
    /// <param name="singleValue">Constant or converted value for unary operators, when applicable.</param>
    /// <param name="listValues">Multiple values for set-style operators such as <c>in</c> / <c>notIn</c>.</param>
    /// <returns>An expression of type <see cref="bool"/> suitable for composition with logical operators.</returns>
    Expression BuildPredicate(Expression memberAccess, Type declaredLeafType, Expression? singleValue,
        IReadOnlyList<Expression>? listValues);
}
