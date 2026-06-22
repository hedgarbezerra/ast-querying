namespace AstQuerying.Queries.Functions.Implementations.Temporal;

/// <inheritdoc />
public abstract class BaseTemporalFunction : BaseFunction<DateTime>
{
    /// <inheritdoc />
    protected override Dictionary<Type, Func<DateTime, object>> Converters => new()
    {
        [typeof(DateTime)] = v => v,
        [typeof(DateOnly)] = v => DateOnly.FromDateTime(v),
        [typeof(TimeOnly)] = v => TimeOnly.FromDateTime(v),
        [typeof(DateTimeOffset)] = v => new DateTimeOffset(v, TimeSpan.Zero)
    };
}