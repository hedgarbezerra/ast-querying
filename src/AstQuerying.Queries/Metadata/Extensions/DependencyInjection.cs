using AstQuerying.Queries.Metadata.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AstQuerying.Queries.Metadata.Extensions;

/// <summary>
///     Extension methods to register query metadata with <see cref="IServiceCollection" />.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Registers <see cref="IEntityMetadataProvider" />.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services" /> instance for chaining.</returns>
    public static IServiceCollection AddEntityMetadata(this IServiceCollection services)
    {
        services.TryAddSingleton<IEntityMetadataProvider, GeneratedEntityMetadataProvider>();
        return services;
    }
}
