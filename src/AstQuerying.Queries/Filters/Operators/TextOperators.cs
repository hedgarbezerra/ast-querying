using System.Linq.Expressions;
using System.Reflection;

namespace AstQuerying.Queries.Filters.Operators;

/// <summary>Substring test mapped to <see cref="string.Contains(string)" /> for EF Core SQL translation.</summary>
public sealed class ContainsOperator : ISingleValueFilterOperator
{
    public const string Key = "contains";

    private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod(
        nameof(string.Contains),
        [typeof(string)])!;

    /// <inheritdoc />
    public string Name => Key;

    /// <inheritdoc />
    public Expression CreateExpression(Expression memberAccess, Expression value)
    {
        if (memberAccess.Type != typeof(string))
            throw new InvalidOperationException("Contains applies to string members only.");

        return Expression.Call(memberAccess, ContainsMethod, value);
    }
}

/// <summary>Prefix test mapped to <see cref="string.StartsWith(string)" /> for EF Core SQL translation.</summary>
public sealed class StartsWithOperator : ISingleValueFilterOperator
{
    public const string Key = "startsWith";

    private static readonly MethodInfo StartsWithMethod = typeof(string).GetMethod(
        nameof(string.StartsWith),
        [typeof(string)])!;

    /// <inheritdoc />
    public string Name => Key;

    /// <inheritdoc />
    public Expression CreateExpression(Expression memberAccess, Expression value)
    {
        if (memberAccess.Type != typeof(string))
            throw new InvalidOperationException("startsWith applies to string members only.");

        return Expression.Call(memberAccess, StartsWithMethod, value);
    }
}

/// <summary>Suffix test mapped to <see cref="string.EndsWith(string)" /> for EF Core SQL translation.</summary>
public sealed class EndsWithOperator : ISingleValueFilterOperator
{
    public const string Key = "endsWith";

    private static readonly MethodInfo EndsWithMethod = typeof(string).GetMethod(
        nameof(string.EndsWith),
        [typeof(string)])!;

    /// <inheritdoc />
    public string Name => Key;

    /// <inheritdoc />
    public Expression CreateExpression(Expression memberAccess, Expression value)
    {
        if (memberAccess.Type != typeof(string))
            throw new InvalidOperationException("endsWith applies to string members only.");

        return Expression.Call(memberAccess, EndsWithMethod, value);
    }
}