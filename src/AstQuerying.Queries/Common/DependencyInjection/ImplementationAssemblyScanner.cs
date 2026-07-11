using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AstQuerying.Queries.Common.DependencyInjection;

/// <summary>
///     Discovers concrete implementations of abstractions declared in the same assembly as the abstraction type.
/// </summary>
internal static class ImplementationAssemblyScanner
{
    /// <summary>
    ///     Returns concrete, non-abstract classes in the abstraction's assembly that implement it.
    /// </summary>
    /// <typeparam name="TAbstraction">The service abstraction (typically an interface).</typeparam>
    /// <returns>Implementation types ordered by full name.</returns>
    public static IReadOnlyList<Type> GetImplementationTypes<TAbstraction>()
        where TAbstraction : class
    {
        var abstraction = typeof(TAbstraction);

        return abstraction.Assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && abstraction.IsAssignableFrom(t))
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    ///     Registers each discovered implementation with <see cref="ServiceDescriptor" /> as an enumerable of
    ///     <typeparamref name="TAbstraction" />.
    /// </summary>
    /// <typeparam name="TAbstraction">The service abstraction (typically an interface).</typeparam>
    /// <param name="services">The service collection.</param>
    public static void RegisterEnumerable<TAbstraction>(IServiceCollection services)
        where TAbstraction : class
    {
        ArgumentNullException.ThrowIfNull(services);

        foreach (var type in GetImplementationTypes<TAbstraction>())
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(TAbstraction), type));
    }
}