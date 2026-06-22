using System.Collections.Frozen;
using AstQuerying.Queries.Filters.Contracts;
using AstQuerying.Queries.Filters.Operators;

namespace AstQuerying.Queries.Registry.Implementations;

/// <summary>
/// Built-in filter operators for registry and DI registration.
/// </summary>
public static class QueryOperatorsRegistry
{
    /// <summary>
    /// Operators keyed by API token (case-insensitive). One instance per operator.
    /// </summary>
    public static readonly FrozenDictionary<string, IFilterOperator> Operators =
        new Dictionary<string, IFilterOperator>(StringComparer.OrdinalIgnoreCase)
        {
            [EqualOperator.Key] = new EqualOperator(),
            [NotEqualOperator.Key] = new NotEqualOperator(),
            [GreaterThanOperator.Key] = new GreaterThanOperator(),
            [GreaterThanOrEqualOperator.Key] = new GreaterThanOrEqualOperator(),
            [LessThanOperator.Key] = new LessThanOperator(),
            [LessThanOrEqualOperator.Key] = new LessThanOrEqualOperator(),
            [ContainsOperator.Key] = new ContainsOperator(),
            [StartsWithOperator.Key] = new StartsWithOperator(),
            [EndsWithOperator.Key] = new EndsWithOperator(),
            [InOperator.Key] = new InOperator(),
            [NotInOperator.Key] = new NotInOperator(),
            [IsNullOperator.Key] = new IsNullOperator(),
            [IsNotNullOperator.Key] = new IsNotNullOperator()
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Maps each built-in operator implementation type to its API token.
    /// </summary>
    private static readonly FrozenDictionary<Type, string> KeysByImplementationType =
        new Dictionary<Type, string>
        {
            [typeof(EqualOperator)] = EqualOperator.Key,
            [typeof(NotEqualOperator)] = NotEqualOperator.Key,
            [typeof(GreaterThanOperator)] = GreaterThanOperator.Key,
            [typeof(GreaterThanOrEqualOperator)] = GreaterThanOrEqualOperator.Key,
            [typeof(LessThanOperator)] = LessThanOperator.Key,
            [typeof(LessThanOrEqualOperator)] = LessThanOrEqualOperator.Key,
            [typeof(ContainsOperator)] = ContainsOperator.Key,
            [typeof(StartsWithOperator)] = StartsWithOperator.Key,
            [typeof(EndsWithOperator)] = EndsWithOperator.Key,
            [typeof(InOperator)] = InOperator.Key,
            [typeof(NotInOperator)] = NotInOperator.Key,
            [typeof(IsNullOperator)] = IsNullOperator.Key,
            [typeof(IsNotNullOperator)] = IsNotNullOperator.Key
        }.ToFrozenDictionary();

    /// <summary>
    /// Distinct operator keys present in <see cref="Operators"/>.
    /// </summary>
    public static readonly IReadOnlyList<string> OperatorKeys = Operators.Keys.ToArray();

    /// <summary>
    /// Resolves the API token for a concrete built-in operator type.
    /// </summary>
    public static string GetKey(Type implementationType)
    {
        ArgumentNullException.ThrowIfNull(implementationType);
        if (KeysByImplementationType.TryGetValue(implementationType, out var key))
            return key;

        throw new InvalidOperationException(
            $"Type '{implementationType.FullName}' is not a registered built-in filter operator.");
    }

    /// <summary>
    /// Resolves the API token for a concrete operator instance.
    /// </summary>
    public static string GetKey(IFilterOperator op)
    {
        ArgumentNullException.ThrowIfNull(op);
        return op.Name;
    }

    /// <summary>
    /// Tries to resolve a built-in operator by API key.
    /// </summary>
    public static bool TryGetOperator(string key, out IFilterOperator? op) => Operators.TryGetValue(key, out op);
}
