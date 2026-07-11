using AstQuerying.Queries.Validation.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AstQuerying.Queries.Validation.Extensions;

/// <summary>
///     Extension methods to register query validation with <see cref="IServiceCollection" />.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Registers <see cref="IQueryValidator" />.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services" /> instance for chaining.</returns>
    public static IServiceCollection AddQueryValidation(this IServiceCollection services)
    {
        services.TryAddSingleton<IQueryValidator, QueryValidator>();
        return services;
    }
}