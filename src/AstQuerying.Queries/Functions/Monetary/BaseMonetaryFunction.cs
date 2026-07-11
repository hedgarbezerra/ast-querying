namespace AstQuerying.Queries.Functions.Monetary;

/// <inheritdoc />
public abstract class BaseMonetaryFunction : BaseFunction<decimal>
{
    /// <summary>
    ///     Centralized mapping of supported integer types to conversion functions for the fixed legal age value.
    /// </summary>
    protected override Dictionary<Type, Func<decimal, object>> Converters => new()
    {
        [typeof(decimal)] = v => v,
        [typeof(double)] = v => (double)v,
        [typeof(float)] = v => (float)v
    };
}