using AstQuerying.Queries.Resolving.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AstQuerying.Queries.Resolving.Extensions;

/// <summary>
///     Extension methods to register property resolving with <see cref="IServiceCollection" />.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Registers <see cref="IPropertyResolver" />.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services" /> instance for chaining.</returns>
    public static IServiceCollection AddQueryResolving(this IServiceCollection services)
    {
        services.TryAddSingleton<IPropertyResolver, PropertyResolver>();
        return services;
    }
}