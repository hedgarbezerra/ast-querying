using System.Collections.Frozen;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Filters;

namespace AstQuerying.Queries.Registries;

/// <summary>
///     Built-in and plugin filter operators registry.
/// </summary>
public sealed class QueryOperatorsRegistry : IOperatorRegistry
{
    /// <summary>
    ///     Initializes a new registry from the supplied operator instances.
    /// </summary>
    /// <param name="operators">Operator instances to index by type and name.</param>
    public QueryOperatorsRegistry(IEnumerable<IFilterOperator> operators)
    {
        ArgumentNullException.ThrowIfNull(operators);

        var byName = new Dictionary<string, IFilterOperator>(StringComparer.OrdinalIgnoreCase);
        var byType = new Dictionary<Type, IFilterOperator>();

        foreach (var op in operators)
        {
            var name = op.Name;
            if (!byName.TryAdd(name, op))
                throw new QueryRegistrationException(name, RegistrationKind.Operator,
                    $"Duplicate filter operator name '{name}'.");

            var type = op.GetType();
            if (!byType.TryAdd(type, op))
                throw new QueryRegistrationException(name, RegistrationKind.Operator,
                    $"Duplicate filter operator implementation type '{type.FullName}'.");
        }

        ByName = byName.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        ByType = byType.ToFrozenDictionary();
    }

    /// <inheritdoc />
    public FrozenDictionary<Type, IFilterOperator> ByType { get; }

    /// <inheritdoc />
    public FrozenDictionary<string, IFilterOperator> ByName { get; }

    /// <inheritdoc />
    public bool TryGetByType(Type implementationType, out IFilterOperator? op)
    {
        return ByType.TryGetValue(implementationType, out op);
    }

    /// <inheritdoc />
    public bool TryGetByName(string name, out IFilterOperator? op)
    {
        return ByName.TryGetValue(name, out op);
    }
}
