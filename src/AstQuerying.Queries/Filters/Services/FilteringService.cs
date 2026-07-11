using AstQuerying.Queries.Ast.ValueObjects;

namespace AstQuerying.Queries.Filters.Services;

/// <summary>
///     Default <see cref="IFilteringService" /> that applies predicates built by <see cref="IFilterExpressionBuilder" />.
/// </summary>
public sealed class FilteringService : IFilteringService
{
    private readonly IFilterExpressionBuilder _filterExpressionBuilder;

    /// <summary>
    ///     Initializes a new instance of <see cref="FilteringService" />.
    /// </summary>
    /// <param name="filterExpressionBuilder">The filter expression builder.</param>
    public FilteringService(IFilterExpressionBuilder filterExpressionBuilder)
    {
        _filterExpressionBuilder = filterExpressionBuilder;
    }

    /// <inheritdoc />
    public IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> source, QueryNode root) where TEntity : class
    {
        var  filterExpression = _filterExpressionBuilder.Build<TEntity>(root);
        return source.Where(filterExpression);
    }
}