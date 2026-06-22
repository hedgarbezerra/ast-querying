using AstQuerying.Queries.Filters.Dtos;
using AstQuerying.Queries.Sorting.Dtos;

namespace AstQuerying.Queries.Common.Pipeline.Contracts;

/// <summary>
/// Applies the standard filter, sort, and pagination flow to an <see cref="IQueryable{T}"/> for a registered entity type.
/// </summary>
public interface IEntityQueryPipeline
{
    /// <summary>
    /// Parses and validates the optional filter, applies ordering (client sort or configured default), then skip/take from pagination rules.
    /// </summary>
    /// <typeparam name="TEntity">The entity root type.</typeparam>
    /// <param name="source">The queryable to extend.</param>
    /// <param name="filter">Optional filter clause tree; when omitted, the entity default filter from metadata is used when configured.</param>
    /// <param name="sort">Optional sort list; when null or empty, the entity default sort from metadata is used.</param>
    /// <param name="pagination">Optional pagination DTO; when null, page 1 and the configured default page size are used.</param>
    /// <returns>The composed queryable.</returns>
    IQueryable<TEntity> Apply<TEntity>(
        IQueryable<TEntity> source,
        FilterClauseDto? filter,
        IReadOnlyList<SortFieldDto>? sort,
        PaginationDto? pagination) where TEntity : class;
}
