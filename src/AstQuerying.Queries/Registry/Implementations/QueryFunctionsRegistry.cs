using System.Collections.Frozen;
using AstQuerying.Queries.Functions.Contracts;
using AstQuerying.Queries.Functions.Implementations.Integer;
using AstQuerying.Queries.Functions.Implementations.Monetary;
using AstQuerying.Queries.Functions.Implementations.Temporal;

namespace AstQuerying.Queries.Registry.Implementations;

/// <summary>
/// Built-in query functions for registry and DI registration.
/// </summary>
public static class QueryFunctionsRegistry
{
    /// <summary>
    /// Functions keyed by API token (case-insensitive). One instance per function.
    /// </summary>
    public static readonly FrozenDictionary<string, IFunction> Functions =
        new Dictionary<string, IFunction>(StringComparer.OrdinalIgnoreCase)
        {
            [TodayFunction.Key] = new TodayFunction(),
            [NowFunction.Key] = new NowFunction(),
            [StartOfMonthFunction.Key] = new StartOfMonthFunction(),
            [EndOfMonthFunction.Key] = new EndOfMonthFunction(),
            [CurrentYearFunction.Key] = new CurrentYearFunction(),
            [LegalAgeFunction.Key] = new LegalAgeFunction(),
            [MinimumWageFunction.Key] = new MinimumWageFunction()
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Maps each built-in function implementation type to its API token.
    /// </summary>
    private static readonly FrozenDictionary<Type, string> KeysByImplementationType =
        new Dictionary<Type, string>
        {
            [typeof(TodayFunction)] = TodayFunction.Key,
            [typeof(NowFunction)] = NowFunction.Key,
            [typeof(StartOfMonthFunction)] = StartOfMonthFunction.Key,
            [typeof(EndOfMonthFunction)] = EndOfMonthFunction.Key,
            [typeof(CurrentYearFunction)] = CurrentYearFunction.Key,
            [typeof(LegalAgeFunction)] = LegalAgeFunction.Key,
            [typeof(MinimumWageFunction)] = MinimumWageFunction.Key
        }.ToFrozenDictionary();

    /// <summary>
    /// Resolves the API token for a concrete built-in function type.
    /// </summary>
    public static string GetKey(Type implementationType)
    {
        ArgumentNullException.ThrowIfNull(implementationType);
        if (KeysByImplementationType.TryGetValue(implementationType, out var key))
            return key;

        throw new InvalidOperationException(
            $"Type '{implementationType.FullName}' is not a registered built-in function.");
    }

    /// <summary>
    /// Resolves the API token for a concrete function instance.
    /// </summary>
    public static string GetKey(IFunction fn)
    {
        ArgumentNullException.ThrowIfNull(fn);
        return fn.Name;
    }

    /// <summary>
    /// Tries to resolve a built-in function by API key.
    /// </summary>
    public static bool TryGetFunction(string key, out IFunction fn) => Functions.TryGetValue(key, out fn);
}
