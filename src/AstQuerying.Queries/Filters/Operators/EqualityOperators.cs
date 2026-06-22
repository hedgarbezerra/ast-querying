using System.Linq.Expressions;
using AstQuerying.Queries.Filters.Contracts;
using AstQuerying.Queries.Registry.Implementations;

namespace AstQuerying.Queries.Filters.Operators;

/// <summary>Equality (<c>=</c>) comparison for scalar members.</summary>
public sealed class EqualOperator : ISingleValueFilterOperator
{
    public const string Key = "=";

    /// <inheritdoc />
    public string Name => Key;

    /// <inheritdoc />
    public Expression CreateExpression(Expression memberAccess, Expression value)
    {
        var right = ExpressionNormalizer.Coerce(value, memberAccess.Type);
        return Expression.Equal(memberAccess, right);
    }
}

/// <summary>Inequality (<c>!=</c>) comparison for scalar members.</summary>
public sealed class NotEqualOperator : ISingleValueFilterOperator
{
    public const string Key = "!=";

    /// <inheritdoc />
    public string Name => Key;

    /// <inheritdoc />
    public Expression CreateExpression(Expression memberAccess, Expression value)
    {
        var right = ExpressionNormalizer.Coerce(value, memberAccess.Type);
        return Expression.NotEqual(memberAccess, right);
    }
}

/// <summary>Greater-than comparison for ordered scalar types.</summary>
public sealed class GreaterThanOperator : ISingleValueFilterOperator
{
    public const string Key = ">";

    /// <inheritdoc />
    public string Name => Key;

    /// <inheritdoc />
    public Expression CreateExpression(Expression memberAccess, Expression value)
    {
        var right = ExpressionNormalizer.Coerce(value, memberAccess.Type);
        return Expression.GreaterThan(memberAccess, right);
    }
}

/// <summary>Greater-than-or-equal comparison for ordered scalar types.</summary>
public sealed class GreaterThanOrEqualOperator : ISingleValueFilterOperator
{
    public const string Key = ">=";

    /// <inheritdoc />
    public string Name => Key;

    /// <inheritdoc />
    public Expression CreateExpression(Expression memberAccess, Expression value)
    {
        var right = ExpressionNormalizer.Coerce(value, memberAccess.Type);
        return Expression.GreaterThanOrEqual(memberAccess, right);
    }
}

/// <summary>Less-than comparison for ordered scalar types.</summary>
public sealed class LessThanOperator : ISingleValueFilterOperator
{
    public const string Key = "<";

    /// <inheritdoc />
    public string Name => Key;

    /// <inheritdoc />
    public Expression CreateExpression(Expression memberAccess, Expression value)
    {
        var right = ExpressionNormalizer.Coerce(value, memberAccess.Type);
        return Expression.LessThan(memberAccess, right);
    }
}

/// <summary>Less-than-or-equal comparison for ordered scalar types.</summary>
public sealed class LessThanOrEqualOperator : ISingleValueFilterOperator
{
    public const string Key = "<=";

    /// <inheritdoc />
    public string Name => Key;

    /// <inheritdoc />
    public Expression CreateExpression(Expression memberAccess, Expression value)
    {
        var right = ExpressionNormalizer.Coerce(value, memberAccess.Type);
        return Expression.LessThanOrEqual(memberAccess, right);
    }
}
