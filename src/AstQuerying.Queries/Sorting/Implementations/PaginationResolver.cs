using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Configuration.DependencyInjection;
using AstQuerying.Queries.Sorting.ValueObjects;

namespace AstQuerying.Queries.Sorting.Implementations;

/// <summary>
/// Normalizes <see cref="PaginationRequestDto"/> into concrete skip/take values using <see cref="QueryEngineOptions"/> limits.
/// </summary>
public static class PaginationResolver
{
    /// <summary>
    /// Resolves pagination from either page-based or skip/take input, enforcing configured maximums.
    /// </summary>
    /// <param name="dto">The incoming pagination DTO.</param>
    /// <param name="options">Engine limits and defaults.</param>
    /// <returns>The resolved skip and take tuple.</returns>
    /// <exception cref="QueryValidationException">When the request is invalid or exceeds limits.</exception>
    public static (int Skip, int Take) Resolve(PaginationRequestDto dto, QueryEngineOptions options)
    {
        var hasPaging = dto.Page is not null || dto.PageSize is not null;
        var hasSkipTake = dto.Skip is not null || dto.Take is not null;

        if (hasPaging && hasSkipTake)
            throw new QueryValidationException("Use either page/pageSize or skip/take, not both.");

        if (hasPaging)
        {
            var page = dto.Page ?? 1;
            var pageSize = dto.PageSize ?? options.DefaultPageSize;
            if (page < 1)
                throw new QueryValidationException("page must be >= 1.");
            if (pageSize < 1)
                throw new QueryValidationException("pageSize must be >= 1.");
            if (pageSize > options.MaxPageSize)
                throw new QueryValidationException($"pageSize must not exceed {options.MaxPageSize}.");

            var skip = (page - 1) * pageSize;
            if (skip > options.MaxSkip)
                throw new QueryValidationException($"skip derived from page must not exceed {options.MaxSkip}.");

            return (skip, pageSize);
        }

        var skip2 = dto.Skip ?? 0;
        var take2 = dto.Take ?? options.DefaultTake;
        if (skip2 < 0)
            throw new QueryValidationException("skip must be >= 0.");
        if (take2 < 1)
            throw new QueryValidationException("take must be >= 1.");
        if (take2 > options.MaxTake)
            throw new QueryValidationException($"take must not exceed {options.MaxTake}.");
        if (skip2 > options.MaxSkip)
            throw new QueryValidationException($"skip must not exceed {options.MaxSkip}.");

        return (skip2, take2);
    }
}
