using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Configurations;
using AstQuerying.Queries.Sorting.Dtos;

namespace AstQuerying.Queries.Sorting.Services;

/// <summary>
///     Normalizes <see cref="PaginationDto" /> into concrete skip/take values using injected
///     <see cref="QueryEngineOptions" /> limits.
/// </summary>
public sealed class PaginationResolver : IPaginationResolver
{
    private readonly QueryEngineOptions _options;

    /// <summary>
    ///     Initializes a new instance of <see cref="PaginationResolver" />.
    /// </summary>
    /// <param name="options">Engine limits and defaults.</param>
    public PaginationResolver(QueryEngineOptions options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public (int Skip, int Take) Resolve(PaginationDto? dto)
    {
        var page = dto?.Page ?? 1;
        var size = dto?.Size ?? _options.DefaultPageSize;

        if (page < 1)
            throw new QueryValidationException("page must be >= 1.");

        if (size < 1)
            throw new QueryValidationException("size must be >= 1.");

        if (size > _options.MaxPageSize)
            throw new QueryValidationException($"size must not exceed {_options.MaxPageSize}.");

        var skip = (page - 1) * size;
        if (skip > _options.MaxSkip)
            throw new QueryValidationException($"skip derived from page must not exceed {_options.MaxSkip}.");

        return (skip, size);
    }
}