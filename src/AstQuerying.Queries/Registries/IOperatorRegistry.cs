using System.Collections.Frozen;
using AstQuerying.Queries.Filters;

namespace AstQuerying.Queries.Registries;

/// <summary>
///     Read-only registry of filter operators keyed by implementation type and API name.
/// </summary>
public interface IOperatorRegistry
{
    /// <summary>
    ///     Gets operators keyed by concrete implementation type.
    /// </summary>
    FrozenDictionary<Type, IFilterOperator> ByType { get; }

    /// <summary>
    ///     Gets operators keyed by API token (case-insensitive).
    /// </summary>
    FrozenDictionary<string, IFilterOperator> ByName { get; }

    /// <summary>
    ///     Tries to resolve an operator by implementation type.
    /// </summary>
    /// <param name="implementationType">The concrete operator type.</param>
    /// <param name="op">The resolved operator when found.</param>
    /// <returns><see langword="true" /> when the type is registered.</returns>
    bool TryGetByType(Type implementationType, out IFilterOperator? op);

    /// <summary>
    ///     Tries to resolve an operator by API token.
    /// </summary>
    /// <param name="name">The operator API token.</param>
    /// <param name="op">The resolved operator when found.</param>
    /// <returns><see langword="true" /> when the name is registered.</returns>
    bool TryGetByName(string name, out IFilterOperator? op);
}