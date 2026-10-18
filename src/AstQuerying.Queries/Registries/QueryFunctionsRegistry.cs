using System.Collections.Frozen;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Functions;

namespace AstQuerying.Queries.Registries;

/// <summary>
///     Built-in and plugin query functions registry.
/// </summary>
public sealed class QueryFunctionsRegistry : IFunctionRegistry
{
    /// <summary>
    ///     Initializes a new registry from the supplied function instances.
    /// </summary>
    /// <param name="functions">Function instances to index by type and name.</param>
    public QueryFunctionsRegistry(IEnumerable<IFunction> functions)
    {
        ArgumentNullException.ThrowIfNull(functions);

        var byName = new Dictionary<string, IFunction>(StringComparer.OrdinalIgnoreCase);
        var byType = new Dictionary<Type, IFunction>();

        foreach (var fn in functions)
        {
            var name = fn.Name;
            if (!byName.TryAdd(name, fn))
                throw new QueryRegistrationException(name, RegistrationKind.Function,
                    $"Duplicate function name '{name}'.");

            var type = fn.GetType();
            if (!byType.TryAdd(type, fn))
                throw new QueryRegistrationException(name, RegistrationKind.Function,
                    $"Duplicate function implementation type '{type.FullName}'.");
        }

        ByName = byName.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        ByType = byType.ToFrozenDictionary();
    }

    /// <inheritdoc />
    public FrozenDictionary<Type, IFunction> ByType { get; }

    /// <inheritdoc />
    public FrozenDictionary<string, IFunction> ByName { get; }

    /// <inheritdoc />
    public bool TryGetByType(Type implementationType, out IFunction? fn)
    {
        return ByType.TryGetValue(implementationType, out fn);
    }

    /// <inheritdoc />
    public bool TryGetByName(string name, out IFunction? fn)
    {
        return ByName.TryGetValue(name, out fn);
    }
}
