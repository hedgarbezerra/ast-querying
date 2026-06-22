using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Common.Pipeline.Contracts;
using AstQuerying.Queries.Filters.Contracts;
using AstQuerying.Queries.Filters.Dtos;
using AstQuerying.Queries.Metadata.ValueObjects;
using AstQuerying.Queries.Ast.Parsing;
using AstQuerying.Queries.Registry.Contracts;
using AstQuerying.Queries.Sorting.Contracts;
using AstQuerying.Queries.Sorting.Dtos;
using AstQuerying.Queries.Validation.Contracts;

namespace AstQuerying.Queries.Common.Pipeline.Implementations;

/// <summary>
/// Default <see cref="IEntityQueryPipeline"/> implementation delegating to the parser, validator, filtering service, sorting service, and pagination resolver.
/// </summary>
public sealed class EntityQueryPipeline : IEntityQueryPipeline
{
    private readonly IQueryRegistry _registry;
    private readonly IQueryValidator _validator;
    private readonly IFilteringService _filteringService;
    private readonly ISortingService _sortingService;
    private readonly IPaginationResolver _paginationResolver;

    /// <summary>
    /// Initializes a new instance of <see cref="EntityQueryPipeline"/>.
    /// </summary>
    public EntityQueryPipeline(
        IQueryRegistry registry,
        IQueryValidator validator,
        IFilteringService filteringService,
        ISortingService sortingService,
        IPaginationResolver paginationResolver)
    {
        _registry = registry;
        _validator = validator;
        _filteringService = filteringService;
        _sortingService = sortingService;
        _paginationResolver = paginationResolver;
    }

    /// <inheritdoc />
    public IQueryable<TEntity> Apply<TEntity>(
        IQueryable<TEntity> source,
        FilterClauseDto? filter,
        IReadOnlyList<SortFieldDto>? sort,
        PaginationDto? pagination) where TEntity : class
    {
        var meta = _registry.GetEntity(typeof(TEntity)) ??
                   throw new QueryValidationException($"No query metadata for {typeof(TEntity).Name}.");

        var filterToApply = ResolveFilterClause(meta.DefaultFilter, filter);
        if (filterToApply is not null)
        {
            var root = QueryFilterParser.Parse(filterToApply);
            _validator.Validate(typeof(TEntity), root);
            source = _filteringService.Apply(source, root);
        }

        var effectiveSort = ResolveSort(sort, meta);
        source = _sortingService.Apply(source, effectiveSort);

        var (skip, take) = _paginationResolver.Resolve(pagination);
        return source.Skip(skip).Take(take);
    }

    private static FilterClauseDto? ResolveFilterClause(FilterClauseDto? defaultFilter, FilterClauseDto? clientFilter) =>
        clientFilter ?? defaultFilter;

    private static IReadOnlyList<SortFieldDto> ResolveSort(IReadOnlyList<SortFieldDto>? sort, EntityMetadata meta)
    {
        if (sort is { Count: > 0 })
            return sort;

        if (meta.DefaultSort is { Count: > 0 })
            return meta.DefaultSort;

        throw new QueryValidationException(
            $"Sort is required for {meta.ClrType.Name}: provide sort fields or configure DefaultSorting on the query configuration.");
    }
}
