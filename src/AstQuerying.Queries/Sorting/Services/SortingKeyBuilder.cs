using System.Linq.Expressions;
using System.Reflection;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Metadata;
using AstQuerying.Queries.Metadata.ValueObjects;
using AstQuerying.Queries.Sorting.Dtos;

namespace AstQuerying.Queries.Sorting.Services;

/// <summary>
///     Default <see cref="ISortingKeyBuilder" /> using generated metadata and collection-aware key selectors.
/// </summary>
public sealed class SortingKeyBuilder : ISortingKeyBuilder
{
    private readonly IEntityMetadataProvider _metadata;

    /// <summary>
    ///     Initializes a new instance of <see cref="SortingKeyBuilder" />.
    /// </summary>
    /// <param name="metadata">The entity metadata provider.</param>
    public SortingKeyBuilder(IEntityMetadataProvider metadata)
    {
        _metadata = metadata;
    }

    /// <inheritdoc />
    public LambdaExpression Build<TEntity>(SortFieldDto field) where TEntity : class
    {
        var entityType = typeof(TEntity);
        var meta = _metadata.GetMetadata(entityType) ??
                   throw new QueryEntityNotQueryableException(entityType);

        var prop = meta.FindByPath(field.Field) ??
                   throw new QueryUnknownFieldException(entityType, field.Field);

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
