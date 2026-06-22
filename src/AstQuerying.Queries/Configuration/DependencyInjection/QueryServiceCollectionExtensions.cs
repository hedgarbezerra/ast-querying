using System.Reflection;
using AstQuerying.Queries.Common.Pipeline.Contracts;
using AstQuerying.Queries.Common.Pipeline.Implementations;
using AstQuerying.Queries.Handlers;
using AstQuerying.Queries.Filters.Contracts;
using AstQuerying.Queries.Filters.Implementations;
using AstQuerying.Queries.Functions.Contracts;
using AstQuerying.Queries.Metadata.Contracts;
using AstQuerying.Queries.Metadata.Implementations;
using AstQuerying.Queries.Registry.Contracts;
using AstQuerying.Queries.Registry.Implementations;
using AstQuerying.Queries.Resolving.Contracts;
using AstQuerying.Queries.Resolving.Implementations;
using AstQuerying.Queries.Sorting.Contracts;
using AstQuerying.Queries.Sorting.Implementations;
using AstQuerying.Queries.Validation.Contracts;
using AstQuerying.Queries.Validation.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AstQuerying.Queries.Configuration.DependencyInjection;

/// <summary>
/// Extension methods to register query engine services with <see cref="IServiceCollection"/>.
/// </summary>
public static class QueryServiceCollectionExtensions
{
    /// <summary>
    /// Registers the query engine and scans the given assemblies for <see cref="T:AstQuerying.Queries.Configuration.QueryConfiguration`1"/> and registers built-in operators and functions from <see cref="QueryOperatorsRegistry"/> and <see cref="QueryFunctionsRegistry"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Assemblies to scan in addition to defaults.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddQueryEngine(this IServiceCollection services, params Assembly[] assemblies)
    {
        return services.AddQueryEngine(o =>
        {
            foreach (var a in assemblies)
                o.AssembliesToScan.Add(a);
        });
    }

    /// <summary>
    /// Registers the query engine. Core services are registered as singletons; built-in filter operators and functions come from
    /// <see cref="QueryOperatorsRegistry"/> and <see cref="QueryFunctionsRegistry"/>.
    /// </summary>
    public static IServiceCollection AddQueryEngine(this IServiceCollection services,
        Action<QueryEngineOptions> configure)
    {
        var options = new QueryEngineOptions();
        configure(options);
        if (options.AssembliesToScan.Count == 0)
            options.AssembliesToScan.Add(Assembly.GetCallingAssembly() ?? typeof(QueryServiceCollectionExtensions).Assembly);

        services.AddSingleton(options);
        services.AddSingleton<IQueryRegistry>(_ => QueryRegistryBuilder.Build(options));
        services.AddSingleton<IQueryValidator, QueryValidator>();
        services.AddSingleton<IValueConverter, ValueConverter>();
        services.AddSingleton<IQueryMetadataProvider, RegistryQueryMetadataProvider>();

        services.AddSingleton<IFilterExpressionBuilder, FilterExpressionBuilder>();
        services.AddSingleton<IFilteringService, FilteringService>();
        services.AddSingleton<ISortingKeyBuilder, SortingKeyBuilder>();
        services.AddSingleton<ISortingService, SortingService>();
        services.AddSingleton<IPaginationResolver, PaginationResolver>();
        services.AddSingleton<IEntityQueryPipeline, EntityQueryPipeline>();
        services.AddSingleton<IPropertyResolver, PropertyResolver>();

        foreach (var op in QueryOperatorsRegistry.Operators.Values)
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IFilterOperator>(op));

        foreach (var fn in QueryFunctionsRegistry.Functions.Values)
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IFunction>(fn));

        return services;
    }

    /// <summary>
    /// Registers <see cref="QueryExceptionHandler"/> with the ASP.NET Core exception handler pipeline and ensures <c>Microsoft.AspNetCore.Http.IProblemDetailsService</c> is available.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddQueryExceptionHandler(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<QueryExceptionHandler>();
        return services;
    }
}
