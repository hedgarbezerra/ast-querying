namespace AstQuerying.Queries.Filters.Contracts;

/// <summary>
/// Converts raw filter literal values from the wire format into CLR values compatible with a property type.
/// </summary>
public interface IValueConverter
{
    /// <summary>
    /// Converts a raw value to <paramref name="targetType"/> using invariant culture rules where applicable.
    /// </summary>
    /// <param name="rawValue">The value from the client payload.</param>
    /// <param name="targetType">The destination type (typically a property leaf type).</param>
    /// <param name="fieldPath">Optional property path for error context.</param>
    /// <returns>The converted value, or <see langword="null"/> when appropriate for nullable types.</returns>
    object? ConvertToType(object? rawValue, Type targetType, string? fieldPath);
}
