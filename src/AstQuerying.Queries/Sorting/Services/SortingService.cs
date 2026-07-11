using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Sorting.Dtos;

namespace AstQuerying.Queries.Sorting.Services;

/// <summary>
///     Default <see cref="ISortingService" /> that composes <c>OrderBy</c> / <c>ThenBy</c> from
///     <see cref="ISortingKeyBuilder" />.
/// </summary>
public sealed class SortingService : ISortingService
{
    private static readonly ConcurrentDictionary<(Type Entity, bool First, bool Desc, Type Key), MethodInfo>
        OrderMethods = new();

    private readonly ISortingKeyBuilder _sortingKeyBuilder;

    /// <summary>
    ///     Initializes a new instance of <see cref="SortingService" />.
    /// </summary>
    /// <param name="sortingKeyBuilder">The sorting key builder.</param>
    public SortingService(ISortingKeyBuilder sortingKeyBuilder)
    {
        _sortingKeyBuilder = sortingKeyBuilder;
    }

    /// <inheritdoc />
    public IOrderedQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> source, IReadOnlyList<SortFieldDto> sort)
        where TEntity : class
    {
        if (sort.Count == 0)
            throw new QueryValidationException("Sort list is empty.");

        IOrderedQueryable<TEntity>? ordered = null;
        for (var i = 0; i < sort.Count; i++)
        {
            var keySelector = _sortingKeyBuilder.Build<TEntity>(sort[i]);
            var descending = sort[i].Direction == SortDirection.Descending;
            var input = i == 0 ? source : ordered!;
            ordered = ApplyQueryableOrdering(input, keySelector, descending, i == 0);
        }

        return ordered!;
    }

    private static IOrderedQueryable<TEntity> ApplyQueryableOrdering<TEntity>(
        IQueryable<TEntity> source,
        LambdaExpression keySelector,
        bool descending,
        bool isFirst) where TEntity : class
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
                ? key.Desc ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy)
                : key.Desc
                    ? nameof(Queryable.ThenByDescending)
                    : nameof(Queryable.ThenBy);

            return typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == methodName && m.IsGenericMethodDefinition)
                .Select(m => new { M = m, P = m.GetParameters() })
                .Single(x =>
                {
                    if (x.P.Length != 2)
                        return false;
                    var p1 = x.P[1].ParameterType;
                    if (!p1.IsGenericType || p1.GetGenericTypeDefinition() != typeof(Expression<>))
                        return false;
                    var funcType = p1.GetGenericArguments()[0];
                    return funcType.IsGenericType && funcType.GetGenericTypeDefinition() == typeof(Func<,>);
                })
                .M.MakeGenericMethod(key.Entity, key.Key);
        });
    }
}