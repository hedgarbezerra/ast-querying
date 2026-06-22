using AstQuerying.Queries.Ast.ValueObjects;

namespace AstQuerying.Queries.Filters.Contracts;

/// <summary>
/// Applies a validated filter AST to an <see cref="IQueryable{T}"/> as a <c>Where</c> predicate.
/// </summary>
public interface IFilteringService
{
    /// <summary>
    /// Builds the predicate from <paramref name="root"/> and applies it to <paramref name="source"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity root type.</typeparam>
    /// <param name="source">The queryable to filter.</param>
    /// <param name="root">The validated filter AST root.</param>
    /// <returns>The filtered queryable.</returns>
    IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> source, QueryNode root) where TEntity : class;
}
