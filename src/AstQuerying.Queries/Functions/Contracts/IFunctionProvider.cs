namespace AstQuerying.Queries.Functions.Contracts;

/// <summary>
/// Supplies a named dynamic value (for example current date/time) that can appear in filter operands.
/// </summary>
/// <remarks>
/// Function names are registered in <see cref="T:AstQuerying.Queries.Registry.Contracts.IQueryRegistry.Functions"/> and referenced from <see cref="T:AstQuerying.Queries.Ast.ValueObjects.FunctionValueNode"/>.
/// </remarks>
public interface IFunctionProvider
{
    /// <summary>
    /// Gets the stable function key used in filter payloads.
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
