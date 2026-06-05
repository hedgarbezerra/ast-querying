using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Metadata.ValueObjects;
using AstQuerying.Queries.Registry.Contracts;
using AstQuerying.Queries.Sorting.Contracts;
using AstQuerying.Queries.Sorting.ValueObjects;

namespace AstQuerying.Queries.Sorting.Implementations;

/// <summary>
/// Default <see cref="ISortBuilder{TEntity}"/> using metadata sort flags and collection-aware key selectors.
/// </summary>
/// <typeparam name="TEntity">The entity root type.</typeparam>
public sealed class SortBuilder<TEntity> : ISortBuilder<TEntity> where TEntity : class
{
    private static readonly ConcurrentDictionary<(Type Entity, bool First, bool Desc, Type Key), MethodInfo> OrderMethods = new();

    private readonly IQueryRegistry _registry;

    /// <summary>
    /// Initializes a new instance of <see cref="SortBuilder{TEntity}"/>.
    /// </summary>
    /// <param name="registry">The query registry.</param>
    public SortBuilder(IQueryRegistry registry)
    {
        _registry = registry;
    }

    /// <inheritdoc />
    public IOrderedQueryable<TEntity> Apply(IQueryable<TEntity> source, IReadOnlyList<SortFieldDto> sort)
    {
        if (sort.Count == 0)
            throw new QueryValidationException("Sort list is empty.");

        IOrderedQueryable<TEntity>? ordered = null;
        for (var i = 0; i < sort.Count; i++)
        {
            var keySelector = BuildKeySelector(sort[i]);
            var descending = string.Equals(sort[i].Direction, "desc", StringComparison.OrdinalIgnoreCase);
            var input = i == 0 ? source : (IQueryable<TEntity>)ordered!;
            ordered = ApplyQueryableOrdering(input, keySelector, descending, i == 0);
        }

        return ordered!;
    }

    /// <inheritdoc />
    public LambdaExpression BuildKeySelector(SortFieldDto field)
    {
        var meta = _registry.GetEntity(typeof(TEntity)) ??
                   throw new QueryValidationException($"No query metadata for {typeof(TEntity).Name}.");

        var prop = meta.FindByPathOrAlias(field.Field) ??
                   throw new QueryValidationException($"Unknown sort field '{field.Field}'.");

        var descending = string.Equals(field.Direction, "desc", StringComparison.OrdinalIgnoreCase);
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
            body = WalkChain(param, prop, 0, prop.Segments.Count);
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

    private static IOrderedQueryable<TEntity> ApplyQueryableOrdering(
        IQueryable<TEntity> source,
        LambdaExpression keySelector,
        bool descending,
        bool isFirst)
    {
        var entityType = typeof(TEntity);
        var keyType = keySelector.ReturnType;
        var method = GetQueryableOrderMethod(isFirst, descending, entityType, keyType);
        var call = Expression.Call(method, source.Expression, Expression.Quote(keySelector));
        return (IOrderedQueryable<TEntity>)source.Provider.CreateQuery(call);
    }

    private static MethodInfo GetQueryableOrderMethod(bool isFirst, bool descending, Type entityType, Type keyType)
    {
        return OrderMethods.GetOrAdd((entityType, isFirst, descending, keyType), key =>
        {
            var methodName = key.First
                ? (key.Desc ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy))
                : (key.Desc ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy));

            return typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == methodName && m.IsGenericMethodDefinition)
                .Select(m => new { M = m, P = m.GetParameters() })
                .Single(x => x.P.Length == 2 && x.P[1].ParameterType.GetGenericArguments().Length == 2)
                .M.MakeGenericMethod(key.Entity, key.Key);
        });
    }

    private static Expression WalkChain(Expression start, PropertyMetadata meta, int from, int to)
    {
        var cur = start;
        for (var i = from; i < to; i++)
            cur = Expression.PropertyOrField(cur, meta.Segments[i].Name);

        return cur;
    }
}
