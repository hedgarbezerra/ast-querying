using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Functions.Contracts;
using AstQuerying.Queries.Registry.Implementations;

namespace AstQuerying.Queries.Configuration.Builders;

/// <summary>
/// Collects allowed dynamic function names for filter operands on a scalar property.
/// </summary>
public sealed class FunctionAllowList
{
    private readonly HashSet<string> _names = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds the function API token for <typeparamref name="T"/> to the allow list.
    /// </summary>
    public FunctionAllowList Allow<T>() where T : class, IFunction, new()
    {
        var key = QueryFunctionsRegistry.GetKey(typeof(T));
        _names.Add(key);
        return this;
    }

    /// <summary>
    /// Adds a built-in function to the allow list by API token (case-insensitive).
    /// </summary>
    public FunctionAllowList Allow(string functionKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(functionKey);
        if (!QueryFunctionsRegistry.TryGetFunction(functionKey, out var fn))
            throw new QueryConfigurationException(
                $"Unknown function '{functionKey}'.",
                new QueryConfigurationDiagnostics { Kind = QueryConfigurationFailureKind.Unspecified });

        _names.Add(fn.Name);
        return this;
    }

    internal IReadOnlySet<string> BuildSet() => _names;
}
