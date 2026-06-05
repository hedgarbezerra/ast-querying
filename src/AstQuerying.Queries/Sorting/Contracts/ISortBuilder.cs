using System.Linq.Expressions;
using AstQuerying.Queries.Sorting.ValueObjects;

namespace AstQuerying.Queries.Sorting.Contracts;

/// <summary>
/// Applies validated sort specifications to an <see cref="IQueryable{T}"/> using metadata from the query registry.
/// </summary>
/// <typeparam name="TEntity">The entity CLR type.</typeparam>
public interface ISortBuilder<TEntity> where TEntity : class
{
    /// <summary>
    /// Applies the given sort sequence to <paramref name="source"/> using <c>OrderBy</c> / <c>ThenBy</c> (or descending variants).
    /// </summary>
    /// <param name="source">The queryable to sort.</param>
    /// <param name="sort">One or more sort fields with direction.</param>
    /// <returns>An ordered queryable reflecting the sort chain.</returns>
    IOrderedQueryable<TEntity> Apply(IQueryable<TEntity> source, IReadOnlyList<SortFieldDto> sort);

    /// <summary>
    /// Builds a key selector expression for a single <paramref name="field"/> (supports simple paths and single-level collection min keys).
    /// </summary>
    /// <param name="field">The sort field descriptor.</param>
    /// <returns>A lambda expression whose body reads the sort key from an entity instance.</returns>
    LambdaExpression BuildKeySelector(SortFieldDto field);
}
