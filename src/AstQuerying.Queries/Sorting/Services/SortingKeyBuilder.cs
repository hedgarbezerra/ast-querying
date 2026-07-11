using System.Linq.Expressions;
using System.Reflection;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Metadata.ValueObjects;
using AstQuerying.Queries.Registries;
using AstQuerying.Queries.Sorting.Dtos;

namespace AstQuerying.Queries.Sorting.Services;

/// <summary>
///     Default <see cref="ISortingKeyBuilder" /> using metadata sort flags and collection-aware key selectors.
/// </summary>
public sealed class SortingKeyBuilder : ISortingKeyBuilder
{
    private readonly IQueryRegistry _registry;

    /// <summary>
    ///     Initializes a new instance of <see cref="SortingKeyBuilder" />.
    /// </summary>
    /// <param name="registry">The query registry.</param>
    public SortingKeyBuilder(IQueryRegistry registry)
    {
        _registry = registry;
    }

    /// <inheritdoc />
    public LambdaExpression Build<TEntity>(SortFieldDto field) where TEntity : class
    {
        var meta = _registry.GetEntity(typeof(TEntity)) ??
                   throw new QueryValidationException($"No query metadata for {typeof(TEntity).Name}.");

        var prop = meta.FindByPathOrAlias(field.Field) ??
                   throw new QueryValidationException($"Unknown sort field '{field.Field}'.");

        var descending = field.Direction == SortDirection.Descending;
        if (descending)
        {
            if (!prop.AllowSortDescending)
                throw new QueryValidationException($"Descending sort is not allowed for field '{field.Field}'.");
        }
        else if (!prop.AllowSortAscending)
        {
            throw new QueryValidationException($"Ascending sort is not allowed for field '{field.Field}'.");
        }

        var param = Expression.Parameter(typeof(TEntity), "e");
        var colIdx = prop.Segments.ToList().FindIndex(s => s.IsCollection);
        Expression body;
        if (colIdx < 0)
        {
            body = WalkChain(param, prop, 0, prop.Segments.Count);
        }
        else
        {
            var coll = WalkChain(param, prop, 0, colIdx + 1);
            var itemType = prop.Segments[colIdx].MemberType;
            var itemParam = Expression.Parameter(itemType, "s");
            var inner = WalkChain(itemParam, prop, colIdx + 1, prop.Segments.Count);
            var keyLambda = Expression.Lambda(inner, itemParam);
            var minMi = GetEnumerableMinMethod(itemType, inner.Type);
            body = Expression.Call(minMi, coll, keyLambda);
        }

        return Expression.Lambda(body, param);
    }

    private static MethodInfo GetEnumerableMinMethod(Type itemType, Type resultType)
    {
        return typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == nameof(Enumerable.Min) && m.IsGenericMethodDefinition)
            .Select(m => new { M = m, P = m.GetParameters() })
            .Single(x => x.P.Length == 2 && x.P[1].ParameterType.IsGenericType &&
                         x.P[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>))
            .M.MakeGenericMethod(itemType, resultType);
    }

    private static Expression WalkChain(Expression start, PropertyMetadata meta, int from, int to)
    {
        var cur = start;
        for (var i = from; i < to; i++)
            cur = Expression.PropertyOrField(cur, meta.Segments[i].Name);

        return cur;
    }
}