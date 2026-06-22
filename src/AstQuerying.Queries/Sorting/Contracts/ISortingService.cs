using AstQuerying.Queries.Sorting.Dtos;

namespace AstQuerying.Queries.Sorting.Contracts;

/// <summary>
/// Applies validated sort specifications to an <see cref="IQueryable{T}"/> using order-by expressions.
/// </summary>
public interface ISortingService
{
    /// <summary>
    /// Applies the given sort sequence to <paramref name="source"/> using <c>OrderBy</c> / <c>ThenBy</c> (or descending variants).
    /// </summary>
    /// <typeparam name="TEntity">The entity root type.</typeparam>
    /// <param name="source">The queryable to sort.</param>
    /// <param name="sort">One or more sort fields with direction.</param>
    /// <returns>An ordered queryable reflecting the sort chain.</returns>
    IOrderedQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> source, IReadOnlyList<SortFieldDto> sort)
        where TEntity : class;
}
