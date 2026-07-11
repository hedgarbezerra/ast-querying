namespace AstQuerying.Queries.Functions.Temporal;

/// <summary>
///     Built-in function for the current UTC calendar date (date component only).
/// </summary>
public sealed class TodayFunction : BaseTemporalFunction
{
    public const string Key = "Today";

    /// <inheritdoc />
    public override string Name => Key;

    /// <inheritdoc />
    public override string DisplayName => "Today";

    /// <inheritdoc />
    public override string Description => "Current date (date component only).";

    /// <inheritdoc />
    protected override DateTime Input => DateTime.UtcNow.Date;
}