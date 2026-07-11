using AstQuerying.Queries.Configurations;
using AstQuerying.Queries.Filters;
using AstQuerying.Queries.Functions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AstQuerying.Queries.Registries.Extensions;

/// <summary>
///     Extension methods to register query registries with <see cref="IServiceCollection" />.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Registers operator and function registries as singletons.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services" /> instance for chaining.</returns>
    public static IServiceCollection AddQuerySupportingRegistries(this IServiceCollection services)
    {
        services.TryAddSingleton<IOperatorRegistry>(sp =>
            new QueryOperatorsRegistry(sp.GetServices<IFilterOperator>()));
        services.TryAddSingleton<IFunctionRegistry>(sp =>
            new QueryFunctionsRegistry(sp.GetServices<IFunction>()));

        return services;
    }

    /// <summary>
    ///     Registers <see cref="IQueryRegistry" /> built from <paramref name="options" />.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">Query engine options used to scan configurations.</param>
    /// <returns>The same <paramref name="services" /> instance for chaining.</returns>
    public static IServiceCollection AddQueryRegistry(this IServiceCollection services, QueryEngineOptions options)
    {
        services.AddSingleton<IQueryRegistry>(sp => QueryRegistryBuilder.Build(
            options,
            sp.GetRequiredService<IOperatorRegistry>(),
            sp.GetRequiredService<IFunctionRegistry>()));

        return services;
    }
}