using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Sorting.Dtos;
using AstQuerying.Queries.Sorting.Internal;

namespace AstQuerying.Queries.Sorting.Services;

/// <summary>
///     Default <see cref="ISortingService" /> that composes <c>OrderBy</c> / <c>ThenBy</c> from
///     <see cref="ISortingKeyBuilder" />.
/// </summary>
public sealed class SortingService : ISortingService
{
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
            throw new QuerySortException(SortViolation.EmptySortList, "Sort list is empty.");

        var first = sort[0];
        var firstKey = _sortingKeyBuilder.Build<TEntity>(first);
        IOrderedQueryable<TEntity> ordered = first.Direction == SortDirection.Descending
            ? QueryableOrdering.OrderByDescending(source, firstKey)
            : QueryableOrdering.OrderBy(source, firstKey);

        for (var i = 1; i < sort.Count; i++)
        {
            var field = sort[i];
            var key = _sortingKeyBuilder.Build<TEntity>(field);
            ordered = field.Direction == SortDirection.Descending
                ? QueryableOrdering.ThenByDescending(ordered, key)
                : QueryableOrdering.ThenBy(ordered, key);
        }

        return ordered;
    }
}
