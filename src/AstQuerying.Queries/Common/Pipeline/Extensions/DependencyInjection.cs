using AstQuerying.Queries.Common.Pipeline.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AstQuerying.Queries.Common.Pipeline.Extensions;

/// <summary>
///     Extension methods to register the entity query pipeline with <see cref="IServiceCollection" />.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Registers <see cref="IEntityQueryPipeline" />.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services" /> instance for chaining.</returns>
    public static IServiceCollection AddQueryPipeline(this IServiceCollection services)
    {
        services.TryAddSingleton<IEntityQueryPipeline, EntityQueryPipeline>();
        return services;
    }
}