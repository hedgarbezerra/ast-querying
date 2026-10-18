using AstQuerying.Queries.Ast.Parsing;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Filters;
using AstQuerying.Queries.Filters.Dtos;
using AstQuerying.Queries.Metadata;
using AstQuerying.Queries.Metadata.ValueObjects;
using AstQuerying.Queries.Sorting;
using AstQuerying.Queries.Sorting.Dtos;
using AstQuerying.Queries.Validation;

namespace AstQuerying.Queries.Common.Pipeline.Services;

/// <summary>
///     Default <see cref="IEntityQueryPipeline" /> implementation delegating to the parser, validator, filtering service,
///     sorting service, and pagination resolver.
/// </summary>
public sealed class EntityQueryPipeline : IEntityQueryPipeline
{
    private readonly IEntityMetadataProvider _metadata;
    private readonly IFilteringService _filteringService;
    private readonly IQueryFilterParser _filterParser;
    private readonly IPaginationResolver _paginationResolver;
    private readonly ISortingService _sortingService;
    private readonly IQueryValidator _validator;

    /// <summary>
    ///     Initializes a new instance of <see cref="EntityQueryPipeline" />.
    /// </summary>
    public EntityQueryPipeline(
        IEntityMetadataProvider metadata,
        IQueryFilterParser filterParser,
        IQueryValidator validator,
        IFilteringService filteringService,
        ISortingService sortingService,
        IPaginationResolver paginationResolver)
    {
        _metadata = metadata;
        _filterParser = filterParser;
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
        var meta = _metadata.GetMetadata(typeof(TEntity)) ??
                   throw new QueryEntityNotQueryableException(typeof(TEntity));

        var filterToApply = filter ?? meta.DefaultFilter;
        if (filterToApply is not null)
        {
            var root = _filterParser.Parse(filterToApply);
            _validator.Validate(typeof(TEntity), root);
            source = _filteringService.Apply(source, root);
        }

        var effectiveSort = ResolveSort(sort, meta);
        if (effectiveSort is { Count: > 0 })
        {
            source = _sortingService.Apply(source, effectiveSort);
        }

        var (skip, take) = _paginationResolver.Resolve(pagination);
        return source.Skip(skip).Take(take);
    }

    private static IReadOnlyList<SortFieldDto> ResolveSort(IReadOnlyList<SortFieldDto>? sort, EntityMetadata meta)
    {
        if (sort is { Count: > 0 })
            return sort;

        if (meta.DefaultSort is { Count: > 0 })
            return meta.DefaultSort;

        return [];
    }
}
