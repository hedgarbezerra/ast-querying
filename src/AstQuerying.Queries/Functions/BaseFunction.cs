using AstQuerying.Queries.Common.Exceptions;

namespace AstQuerying.Queries.Functions;

/// <inheritdoc />
public abstract class BaseFunction<T> : IFunction
{
    /// <summary>
    ///     Defines implementation specific value to be converted
    /// </summary>
    protected abstract T Input { get; }

    /// <summary>
    ///     Centralized mapping of supported types to conversion functions for the given type.
    /// </summary>
    protected abstract Dictionary<Type, Func<T, object>> Converters { get; }

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract string DisplayName { get; }

    /// <inheritdoc />
    public abstract string Description { get; }

    /// <inheritdoc />
    public IReadOnlyList<Type> SupportedTypes => Converters.Keys.ToArray();

    /// <inheritdoc />
    public object? GetValue(Type targetType)
    {
        if (!Converters.TryGetValue(targetType, out var convert))
            throw new QueryConversionException($"{GetType().Name} does not support {targetType.Name}.");

        return convert(Input);
    }
}