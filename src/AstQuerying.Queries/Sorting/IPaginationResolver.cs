using AstQuerying.Queries.Sorting.Dtos;

namespace AstQuerying.Queries.Sorting;

/// <summary>
///     Resolves <see cref="PaginationDto" /> into skip/take using injected engine limits.
/// </summary>
public interface IPaginationResolver
{
    /// <summary>
    ///     Resolves one-based page and size into skip and take, enforcing configured maximums.
    /// </summary>
    /// <param name="dto">The incoming pagination DTO.</param>
    /// <returns>The resolved skip and take tuple.</returns>
    (int Skip, int Take) Resolve(PaginationDto? dto);
}