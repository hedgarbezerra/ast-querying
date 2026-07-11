using AstQuerying.Queries.Common.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AstQuerying.Queries.Functions.Extensions;

/// <summary>
///     Extension methods to register query functions with <see cref="IServiceCollection" />.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Registers <see cref="IFunction" /> implementations discovered in the same assembly as the interface.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services" /> instance for chaining.</returns>
    public static IServiceCollection AddFunctions(this IServiceCollection services)
    {
        ImplementationAssemblyScanner.RegisterEnumerable<IFunction>(services);
        return services;
    }
}