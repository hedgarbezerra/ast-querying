using System.Reflection;
using AstQuerying.Queries.Ast.Parsing.Extensions;
using AstQuerying.Queries.Common.Pipeline.Extensions;
using AstQuerying.Queries.Filters.Extensions;
using AstQuerying.Queries.Functions.Extensions;
using AstQuerying.Queries.Metadata.Extensions;
using AstQuerying.Queries.Registries.Extensions;
using AstQuerying.Queries.Resolving.Extensions;
using AstQuerying.Queries.Sorting.Extensions;
using AstQuerying.Queries.Validation.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace AstQuerying.Queries.Configurations.Extensions;

/// <summary>
///     Extension methods to register query engine services with <see cref="IServiceCollection" />.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Registers the query engine and scans the given assemblies for <see cref="QueryConfiguration{TEntity}" /> and
    ///     registers built-in operators and functions.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Assemblies to scan in addition to defaults.</param>
    /// <returns>The same <paramref name="services" /> instance for chaining.</returns>
    public static IServiceCollection AddQueryEngine(this IServiceCollection services, params Assembly[] assemblies)
    {
        return services.AddQueryEngine(o =>
        {
            foreach (var a in assemblies)
                o.AssembliesToScan.Add(a);
        });
    }

    /// <summary>
    ///     Registers the query engine by delegating to each domain <c>Extensions.DependencyInjection</c>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Callback to configure <see cref="QueryEngineOptions" />.</param>
    /// <returns>The same <paramref name="services" /> instance for chaining.</returns>
    public static IServiceCollection AddQueryEngine(this IServiceCollection services,
        Action<QueryEngineOptions> configure)
    {
        var options = new QueryEngineOptions();
        configure(options);
        if (options.AssembliesToScan.Count == 0)
            options.AssembliesToScan.Add(Assembly.GetCallingAssembly() ?? typeof(DependencyInjection).Assembly);

        services.AddSingleton(options);
        services.AddFilters();
        services.AddFunctions();
        services.AddQuerySupportingRegistries();
        services.AddQueryRegistry(options);
        services.AddQueryParsing();
        services.AddQueryValidation();
        services.AddQuerySorting();
        services.AddQueryMetadata();
        services.AddQueryResolving();
        services.AddQueryPipeline();

        return services;
    }
}