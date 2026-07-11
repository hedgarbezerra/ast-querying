using AstQuerying.Queries.Common.DependencyInjection;
using AstQuerying.Queries.Filters.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AstQuerying.Queries.Filters.Extensions;

/// <summary>
///     Extension methods to register filter services with <see cref="IServiceCollection" />.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Registers filter operators discovered in the <see cref="IFilterOperator" /> assembly and filter expression
    ///     services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services" /> instance for chaining.</returns>
    public static IServiceCollection AddFilters(this IServiceCollection services)
    {
        ImplementationAssemblyScanner.RegisterEnumerable<IFilterOperator>(services);

        services.TryAddSingleton<IFilterExpressionBuilder, FilterExpressionBuilder>();
        services.TryAddSingleton<IFilteringService, FilteringService>();
        services.TryAddSingleton<IValueConverter, ValueConverter>();

        return services;
    }
}