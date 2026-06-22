namespace AstQuerying.Queries.Functions.Contracts;

/// <summary>
/// Supplies a named dynamic value (for example current date/time) that can appear in filter operands.
/// </summary>
/// <remarks>
/// Each concrete implementation declares <c>public const string Key</c> as the API token and exposes the same value via <see cref="Name"/>.
/// </remarks>
public interface IFunction
{
    /// <summary>
    /// Gets the API token for this function (same value as the type&apos;s <c>public const string Key</c>).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a short human-readable label for documentation or UI.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets a longer description of semantics and supported types.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the CLR types this function can produce when coerced to a property type.
    /// </summary>
    IReadOnlyList<Type> SupportedTypes { get; }

    /// <summary>
    /// Resolves the runtime value for the requested <paramref name="targetType"/>.
    /// </summary>
    /// <param name="targetType">The leaf property type the value should be compatible with.</param>
    /// <returns>The resolved value, or <see langword="null"/> when not applicable.</returns>
    object? GetValue(Type targetType);
}
