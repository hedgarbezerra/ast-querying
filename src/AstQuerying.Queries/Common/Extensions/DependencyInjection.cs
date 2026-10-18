using AstQuerying.Queries.Ast.Parsing.Extensions;
using AstQuerying.Queries.Common;
using AstQuerying.Queries.Common.Pipeline.Extensions;
using AstQuerying.Queries.Filters.Extensions;
using AstQuerying.Queries.Functions.Extensions;
using AstQuerying.Queries.Metadata.Extensions;
using AstQuerying.Queries.Registries.Extensions;
using AstQuerying.Queries.Resolving.Extensions;
using AstQuerying.Queries.Sorting.Extensions;
using AstQuerying.Queries.Validation.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace AstQuerying.Queries.Common.Extensions;

/// <summary>
///     Extension methods to register query engine services with <see cref="IServiceCollection" />.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Registers the query engine by delegating to each domain extension.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Callback to configure <see cref="QueryOptions" />.</param>
    /// <returns>The same <paramref name="services" /> instance for chaining.</returns>
    public static IServiceCollection AddQueryEngine(this IServiceCollection services, Action<QueryOptions> configure)
    {
        var options = new QueryOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddFilters();
        services.AddFunctions();
        services.AddQuerySupportingRegistries();
        services.AddEntityMetadata();
        services.AddQueryParsing();
        services.AddQueryValidation();
        services.AddQuerySorting();
        services.AddQueryResolving();
        services.AddQueryPipeline();

        return services;
    }
}
