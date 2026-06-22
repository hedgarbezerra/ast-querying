using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Filters.Contracts;
using AstQuerying.Queries.Registry.Implementations;

namespace AstQuerying.Queries.Configuration.Builders;

/// <summary>
/// Collects allowed filter operator names for a scalar property.
/// </summary>
public sealed class OperatorAllowList
{
    private readonly HashSet<string> _names = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds the operator API token for <typeparamref name="T"/> to the allow list.
    /// </summary>
    public OperatorAllowList Allow<T>() where T : class, IFilterOperator, new()
    {
        var key = QueryOperatorsRegistry.GetKey(typeof(T));
        _names.Add(key);
        return this;
    }

    /// <summary>
    /// Adds a built-in operator to the allow list by API token (case-insensitive).
    /// </summary>
    public OperatorAllowList Allow(string operatorKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operatorKey);
        if (!QueryOperatorsRegistry.TryGetOperator(operatorKey, out var op))
            throw new QueryConfigurationException(
                $"Unknown filter operator '{operatorKey}'.",
                new QueryConfigurationDiagnostics { Kind = QueryConfigurationFailureKind.Unspecified });

        _names.Add(op.Name);
        return this;
    }

    internal IReadOnlySet<string> BuildSet() => _names;
}
