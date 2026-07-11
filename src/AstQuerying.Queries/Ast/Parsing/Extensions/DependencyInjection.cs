using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AstQuerying.Queries.Ast.Parsing.Extensions;

/// <summary>
///     Extension methods to register query filter parsing with <see cref="IServiceCollection" />.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Registers <see cref="IQueryFilterParser" />.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services" /> instance for chaining.</returns>
    public static IServiceCollection AddQueryParsing(this IServiceCollection services)
    {
        services.TryAddSingleton<IQueryFilterParser, QueryFilterParser>();
        return services;
    }
}