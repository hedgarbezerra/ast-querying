using System.Collections.Frozen;
using AstQuerying.Queries.Functions;

namespace AstQuerying.Queries.Registries;

/// <summary>
///     Read-only registry of query functions keyed by implementation type and API name.
/// </summary>
public interface IFunctionRegistry
{
    /// <summary>
    ///     Gets functions keyed by concrete implementation type.
    /// </summary>
    FrozenDictionary<Type, IFunction> ByType { get; }

    /// <summary>
    ///     Gets functions keyed by API token (case-insensitive).
    /// </summary>
    FrozenDictionary<string, IFunction> ByName { get; }

    /// <summary>
    ///     Tries to resolve a function by implementation type.
    /// </summary>
    /// <param name="implementationType">The concrete function type.</param>
    /// <param name="fn">The resolved function when found.</param>
    /// <returns><see langword="true" /> when the type is registered.</returns>
    bool TryGetByType(Type implementationType, out IFunction? fn);

    /// <summary>
    ///     Tries to resolve a function by API token.
    /// </summary>
    /// <param name="name">The function API token.</param>
    /// <param name="fn">The resolved function when found.</param>
    /// <returns><see langword="true" /> when the name is registered.</returns>
    bool TryGetByName(string name, out IFunction? fn);
}