using System.Linq.Expressions;
using AstQuerying.Queries.Registries;

namespace AstQuerying.Queries.Filters.Operators;

/// <summary>
///     Base for <c>in</c> / <c>notIn</c>; builds
///     <see cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource)" /> over a constant array for EF Core
///     <c>IN</c> translation.
/// </summary>
public abstract class CollectionsBaseOperator : IMultiValueFilterOperator
{
    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public Expression CreateExpression(Expression memberAccess, IReadOnlyList<Expression> values)
    {
        if (values.Count == 0)
            throw new InvalidOperationException($"{GetType().Name} operator requires values.");

        var containsInSet = CreateContainsExpression(memberAccess, values);
        return FinalizeContains(containsInSet);
    }

    /// <summary>Hook for derived operators.</summary>
    protected abstract Expression FinalizeContains(Expression containsInSet);

    private static Expression CreateContainsExpression(Expression memberAccess, IReadOnlyList<Expression> values)
    {
        var elemType = Nullable.GetUnderlyingType(memberAccess.Type) ?? memberAccess.Type;
        var arrayInit = Expression.NewArrayInit(elemType, values.Select(v => ExpressionNormalizer.Coerce(v, elemType)));
        var memberCoerced = ExpressionNormalizer.Coerce(memberAccess, elemType);
        return Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Contains),
            [elemType],
            arrayInit,
            memberCoerced);
    }
}

/// <summary>Set membership for EF Core <c>IN</c> translation.</summary>
public sealed class InOperator : CollectionsBaseOperator
{
    public const string Key = "in";

    /// <inheritdoc />
    public override string Name => Key;

    /// <inheritdoc />
    protected override Expression FinalizeContains(Expression containsInSet)
    {
        return containsInSet;
    }
}

/// <summary>Negated set membership for EF Core translation.</summary>
public sealed class NotInOperator : CollectionsBaseOperator
{
    public const string Key = "notIn";

    /// <inheritdoc />
    public override string Name => Key;

    /// <inheritdoc />
    protected override Expression FinalizeContains(Expression containsInSet)
    {
        return Expression.Not(containsInSet);
    }
}