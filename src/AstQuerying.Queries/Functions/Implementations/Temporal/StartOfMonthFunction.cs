using AstQuerying.Queries.Functions.Contracts;

namespace AstQuerying.Queries.Functions.Implementations.Temporal;

/// <summary>
/// Built-in provider for the first instant of the current UTC month.
/// </summary>
public sealed class StartOfMonthFunction : BaseTemporalFunction
{
    public const string Key = "StartOfMonth";

    /// <inheritdoc />
    /// <inheritdoc />
    public override string Name => Key;

    /// <inheritdoc />
    public override string DisplayName => "Start of month";

    /// <inheritdoc />
    public override string Description => "First day of the current month.";

    /// <inheritdoc />
    protected override DateTime Input
    {
        get
        {
            var today = DateTime.UtcNow;
            var start = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            return start;
        }
    }
}
