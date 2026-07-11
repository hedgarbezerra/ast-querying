using AstQuerying.Queries.Sorting.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AstQuerying.Queries.Sorting.Extensions;

/// <summary>
///     Extension methods to register query sorting with <see cref="IServiceCollection" />.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Registers sorting and pagination services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services" /> instance for chaining.</returns>
    public static IServiceCollection AddQuerySorting(this IServiceCollection services)
    {
        services.TryAddSingleton<ISortingKeyBuilder, SortingKeyBuilder>();
        services.TryAddSingleton<ISortingService, SortingService>();
        services.TryAddSingleton<IPaginationResolver, PaginationResolver>();
        return services;
    }
}