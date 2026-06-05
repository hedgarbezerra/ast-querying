using System.Reflection;
using AstQuerying.Queries.Common.AspNetCore;
using AstQuerying.Queries.Configuration.Discovery;
using AstQuerying.Queries.Filters.Contracts;
using AstQuerying.Queries.Filters.Implementations;
using AstQuerying.Queries.Functions.Contracts;
using AstQuerying.Queries.Functions.Implementations;
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
    /// Registers the query engine and scans the given assemblies for <see cref="T:AstQuerying.Queries.Configuration.QueryConfiguration`1"/> and optional plugin operators/functions.
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
    /// Registers the query engine. Open-generic infrastructure services (builders, resolver, metadata provider)
    /// stay as singleton closed per entity at resolve time; filter operators and function providers are resolved from
    /// assembly scan plus built-in implementations.
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

        services.AddSingleton(typeof(IFilterExpressionBuilder<>), typeof(FilterExpressionBuilder<>));
        services.AddSingleton(typeof(ISortBuilder<>), typeof(SortBuilder<>));
        services.AddSingleton(typeof(IPropertyResolver<>), typeof(PropertyResolver<>));
        services.AddSingleton(typeof(IQueryMetadataProvider<>), typeof(QueryMetadataProvider<>));

        foreach (var op in BuiltInOperators)
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IFilterOperator>(op));

        foreach (var type in QueryAssemblyScanner.DiscoverPluginTypes(options, typeof(IFilterOperator)))
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IFilterOperator>((IFilterOperator)Activator.CreateInstance(type)!));

        foreach (var fn in BuiltInFunctions)
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IFunctionProvider>(fn));

        foreach (var type in QueryAssemblyScanner.DiscoverPluginTypes(options, typeof(IFunctionProvider)))
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IFunctionProvider>((IFunctionProvider)Activator.CreateInstance(type)!));

        return services;
    }

    /// <summary>
    /// Registers <see cref="QueryExceptionHandler"/> with the ASP.NET Core exception handler pipeline.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddQueryExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<QueryExceptionHandler>();
        return services;
    }

    private static readonly IFilterOperator[] BuiltInOperators =
    {
        new EqualOperator(),
        new NotEqualOperator(),
        new GreaterThanOperator(),
        new GreaterThanOrEqualOperator(),
        new LessThanOperator(),
        new LessThanOrEqualOperator(),
        new ContainsOperator(),
        new StartsWithOperator(),
        new EndsWithOperator(),
        new InOperator(),
        new NotInOperator(),
        new IsNullOperator(),
        new IsNotNullOperator()
    };

    private static readonly IFunctionProvider[] BuiltInFunctions =
    {
        new TodayFunction(),
        new NowFunction(),
        new StartOfMonthFunction(),
        new EndOfMonthFunction(),
        new CurrentYearFunction()
    };
}
