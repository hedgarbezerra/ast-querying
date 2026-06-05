using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AstQuerying.Queries.Filters.Contracts;
using AstQuerying.Queries.Registry.Implementations;

namespace AstQuerying.Queries.Filters.Implementations;

/// <summary>Equality (<c>=</c>) comparison for scalar members.</summary>
public sealed class EqualOperator : IFilterOperator
{
    /// <inheritdoc />
    public string Name => "=";

    /// <inheritdoc />
    public Expression BuildPredicate(Expression memberAccess, Type declaredLeafType, Expression? singleValue,
        IReadOnlyList<Expression>? listValues)
    {
        if (singleValue is null)
            throw new InvalidOperationException("Equal operator requires a value.");

        var right = ExpressionNormalizer.Coerce(singleValue, memberAccess.Type);
        return Expression.Equal(memberAccess, right);
    }
}

/// <summary>Inequality (<c>!=</c>) comparison for scalar members.</summary>
public sealed class NotEqualOperator : IFilterOperator
{
    /// <inheritdoc />
    public string Name => "!=";

    /// <inheritdoc />
    public Expression BuildPredicate(Expression memberAccess, Type declaredLeafType, Expression? singleValue,
        IReadOnlyList<Expression>? listValues)
    {
        if (singleValue is null)
            throw new InvalidOperationException("NotEqual operator requires a value.");

        var right = ExpressionNormalizer.Coerce(singleValue, memberAccess.Type);
        return Expression.NotEqual(memberAccess, right);
    }
}

/// <summary>Greater-than comparison for ordered scalar types.</summary>
public sealed class GreaterThanOperator : IFilterOperator
{
    /// <inheritdoc />
    public string Name => ">";

    /// <inheritdoc />
    public Expression BuildPredicate(Expression memberAccess, Type declaredLeafType, Expression? singleValue,
        IReadOnlyList<Expression>? listValues)
    {
        if (singleValue is null)
            throw new InvalidOperationException("GreaterThan operator requires a value.");

        var right = ExpressionNormalizer.Coerce(singleValue, memberAccess.Type);
        return Expression.GreaterThan(memberAccess, right);
    }
}

/// <summary>Greater-than-or-equal comparison for ordered scalar types.</summary>
public sealed class GreaterThanOrEqualOperator : IFilterOperator
{
    /// <inheritdoc />
    public string Name => ">=";

    /// <inheritdoc />
    public Expression BuildPredicate(Expression memberAccess, Type declaredLeafType, Expression? singleValue,
        IReadOnlyList<Expression>? listValues)
    {
        if (singleValue is null)
            throw new InvalidOperationException("GreaterThanOrEqual operator requires a value.");

        var right = ExpressionNormalizer.Coerce(singleValue, memberAccess.Type);
        return Expression.GreaterThanOrEqual(memberAccess, right);
    }
}

/// <summary>Less-than comparison for ordered scalar types.</summary>
public sealed class LessThanOperator : IFilterOperator
{
    /// <inheritdoc />
    public string Name => "<";

    /// <inheritdoc />
    public Expression BuildPredicate(Expression memberAccess, Type declaredLeafType, Expression? singleValue,
        IReadOnlyList<Expression>? listValues)
    {
        if (singleValue is null)
            throw new InvalidOperationException("LessThan operator requires a value.");

        var right = ExpressionNormalizer.Coerce(singleValue, memberAccess.Type);
        return Expression.LessThan(memberAccess, right);
    }
}

/// <summary>Less-than-or-equal comparison for ordered scalar types.</summary>
public sealed class LessThanOrEqualOperator : IFilterOperator
{
    /// <inheritdoc />
    public string Name => "<=";

    /// <inheritdoc />
    public Expression BuildPredicate(Expression memberAccess, Type declaredLeafType, Expression? singleValue,
        IReadOnlyList<Expression>? listValues)
    {
        if (singleValue is null)
            throw new InvalidOperationException("LessThanOrEqual operator requires a value.");

        var right = ExpressionNormalizer.Coerce(singleValue, memberAccess.Type);
        return Expression.LessThanOrEqual(memberAccess, right);
    }
}

/// <summary>Substring test using ordinal <see cref="string.Contains(string, StringComparison)"/>.</summary>
public sealed class ContainsOperator : IFilterOperator
{
    /// <inheritdoc />
    public string Name => "contains";

    /// <inheritdoc />
    public Expression BuildPredicate(Expression memberAccess, Type declaredLeafType, Expression? singleValue,
        IReadOnlyList<Expression>? listValues)
    {
        if (singleValue is null)
            throw new InvalidOperationException("Contains operator requires a value.");

        if (memberAccess.Type != typeof(string))
            throw new InvalidOperationException("Contains applies to string members only.");

        var m = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string), typeof(StringComparison) });
        return Expression.Call(memberAccess, m!, singleValue, Expression.Constant(StringComparison.Ordinal));
    }
}

/// <summary>Prefix test for string members using ordinal comparison.</summary>
public sealed class StartsWithOperator : IFilterOperator
{
    /// <inheritdoc />
    public string Name => "startsWith";

    /// <inheritdoc />
    public Expression BuildPredicate(Expression memberAccess, Type declaredLeafType, Expression? singleValue,
        IReadOnlyList<Expression>? listValues)
    {
        if (singleValue is null)
            throw new InvalidOperationException("startsWith operator requires a value.");

        if (memberAccess.Type != typeof(string))
            throw new InvalidOperationException("startsWith applies to string members only.");

        var m = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string), typeof(StringComparison) });
        return Expression.Call(memberAccess, m!, singleValue, Expression.Constant(StringComparison.Ordinal));
    }
}

/// <summary>Suffix test for string members using ordinal comparison.</summary>
public sealed class EndsWithOperator : IFilterOperator
{
    /// <inheritdoc />
    public string Name => "endsWith";

    /// <inheritdoc />
    public Expression BuildPredicate(Expression memberAccess, Type declaredLeafType, Expression? singleValue,
        IReadOnlyList<Expression>? listValues)
    {
        if (singleValue is null)
            throw new InvalidOperationException("endsWith operator requires a value.");

        if (memberAccess.Type != typeof(string))
            throw new InvalidOperationException("endsWith applies to string members only.");

        var m = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string), typeof(StringComparison) });
        return Expression.Call(memberAccess, m!, singleValue, Expression.Constant(StringComparison.Ordinal));
    }
}

/// <summary>Set membership using <see cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource)"/> over a materialized array of operands.</summary>
public sealed class InOperator : IFilterOperator
{
    /// <inheritdoc />
    public string Name => "in";

    /// <inheritdoc />
    public Expression BuildPredicate(Expression memberAccess, Type declaredLeafType, Expression? singleValue,
        IReadOnlyList<Expression>? listValues)
    {
        if (listValues is null || listValues.Count == 0)
            throw new InvalidOperationException("in operator requires values.");

        var elemType = Nullable.GetUnderlyingType(memberAccess.Type) ?? memberAccess.Type;
        var arrayType = elemType.MakeArrayType();
        var init = Expression.NewArrayInit(elemType, listValues.Select(v => ExpressionNormalizer.Coerce(v, elemType)));
        var arr = Expression.Variable(arrayType, "arr");
        var assign = Expression.Assign(arr, init);
        var containsMi = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2)
            .MakeGenericMethod(elemType);
        var call = Expression.Call(containsMi, arr, ExpressionNormalizer.Coerce(memberAccess, elemType));
        return Expression.Block(new[] { arr }, assign, call);
    }
}

/// <summary>Negated set membership (member must not appear in the operand list).</summary>
public sealed class NotInOperator : IFilterOperator
{
    /// <inheritdoc />
    public string Name => "notIn";

    /// <inheritdoc />
    public Expression BuildPredicate(Expression memberAccess, Type declaredLeafType, Expression? singleValue,
        IReadOnlyList<Expression>? listValues)
    {
        if (listValues is null || listValues.Count == 0)
            throw new InvalidOperationException("notIn operator requires values.");

        var elemType = Nullable.GetUnderlyingType(memberAccess.Type) ?? memberAccess.Type;
        var arrayType = elemType.MakeArrayType();
        var arr = Expression.Variable(arrayType, "arr");
        var init = Expression.NewArrayInit(elemType, listValues.Select(v => ExpressionNormalizer.Coerce(v, elemType)));
        var assign = Expression.Assign(arr, init);
        var containsMi = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2)
            .MakeGenericMethod(elemType);
        var call = Expression.Call(containsMi, arr, ExpressionNormalizer.Coerce(memberAccess, elemType));
        return Expression.Block(new[] { arr }, assign, Expression.Not(call));
    }
}

/// <summary>Null or missing-value test with correct semantics for nullable value types and reference types.</summary>
public sealed class IsNullOperator : IFilterOperator
{
    /// <inheritdoc />
    public string Name => "isNull";

    /// <inheritdoc />
    public Expression BuildPredicate(Expression memberAccess, Type declaredLeafType, Expression? singleValue,
        IReadOnlyList<Expression>? listValues)
    {
        return NullSemantics.BuildIsNull(memberAccess);
    }
}

/// <summary>Non-null test; the logical negation of <see cref="IsNullOperator"/>.</summary>
public sealed class IsNotNullOperator : IFilterOperator
{
    /// <inheritdoc />
    public string Name => "isNotNull";

    /// <inheritdoc />
    public Expression BuildPredicate(Expression memberAccess, Type declaredLeafType, Expression? singleValue,
        IReadOnlyList<Expression>? listValues)
    {
        return Expression.Not(NullSemantics.BuildIsNull(memberAccess));
    }
}

internal static class NullSemantics
{
    public static Expression BuildIsNull(Expression memberAccess)
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
