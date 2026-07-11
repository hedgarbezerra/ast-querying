using Microsoft.Extensions.DependencyInjection;

namespace AstQuerying.Queries.Handlers.Extensions;

/// <summary>
///     Extension methods to register query exception handling with <see cref="IServiceCollection" />.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Registers <see cref="QueryExceptionHandler" /> with the ASP.NET Core exception handler pipeline.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services" /> instance for chaining.</returns>
    public static IServiceCollection AddQueryExceptionHandler(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<QueryExceptionHandler>();
        return services;
    }
}