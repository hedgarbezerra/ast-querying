using AstQuerying.Queries.Sorting.Dtos;

namespace AstQuerying.Queries.Sorting;

/// <summary>
///     Applies validated sort specifications to an <see cref="IQueryable{T}" /> using order-by expressions.
/// </summary>
public interface ISortingService
{
    /// <summary>
    ///     Applies the given sort sequence to <paramref name="source" /> using <c>OrderBy</c> / <c>ThenBy</c> (or descending
    ///     variants). The first entry becomes <c>OrderBy</c>; subsequent entries are applied as <c>ThenBy</c> in list order.
    /// </summary>
    /// <typeparam name="TEntity">The entity root type.</typeparam>
    /// <param name="source">The queryable to sort.</param>
    /// <param name="sort">Sort fields in <c>OrderBy</c> then <c>ThenBy</c> order.</param>
    /// <returns>An ordered queryable reflecting the sort chain.</returns>
    IOrderedQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> source, IReadOnlyList<SortFieldDto> sort)
        where TEntity : class;
}