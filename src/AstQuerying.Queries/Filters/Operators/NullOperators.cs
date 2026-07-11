using System.Linq.Expressions;

namespace AstQuerying.Queries.Filters.Operators;

/// <summary>
///     Base for <c>isNull</c> / <c>isNotNull</c> with shared null semantics for nullable value types and reference
///     types.
/// </summary>
public abstract class NullOperatorBase : IMemberOnlyFilterOperator
{
    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public Expression CreateExpression(Expression memberAccess)
    {
        return FinalizeIsNull(CreateNullExpression(memberAccess));
    }

    /// <summary>Hook for derived operators.</summary>
    protected abstract Expression FinalizeIsNull(Expression isNullExpression);

    private static Expression CreateNullExpression(Expression memberAccess)
    {
        var type = memberAccess.Type;
        if (Nullable.GetUnderlyingType(type) is not null)
        {
            var hasValue = Expression.Property(memberAccess, nameof(Nullable<int>.HasValue));
            return Expression.Not(hasValue);
        }

        if (!type.IsValueType)
            return Expression.Equal(memberAccess, Expression.Constant(null, type));

        return Expression.Constant(false);
    }
}

/// <summary>Null or missing-value test with correct semantics for nullable value types and reference types.</summary>
public sealed class IsNullOperator : NullOperatorBase
{
    public const string Key = "isNull";

    /// <inheritdoc />
    public override string Name => Key;

    /// <inheritdoc />
    protected override Expression FinalizeIsNull(Expression isNullExpression)
    {
        return isNullExpression;
    }
}

/// <summary>Non-null test; the logical negation of <see cref="IsNullOperator" />.</summary>
public sealed class IsNotNullOperator : NullOperatorBase
{
    public const string Key = "isNotNull";

    /// <inheritdoc />
    public override string Name => Key;

    /// <inheritdoc />
    protected override Expression FinalizeIsNull(Expression isNullExpression)
    {
        return Expression.Not(isNullExpression);
    }
}