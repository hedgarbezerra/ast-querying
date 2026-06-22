using AstQuerying.Queries.Functions.Contracts;

namespace AstQuerying.Queries.Functions.Implementations.Temporal;

/// <summary>
/// Built-in provider for the last representable instant of the current UTC calendar month.
/// </summary>
public sealed class EndOfMonthFunction : BaseTemporalFunction
{
    public const string Key = "EndOfMonth";

    /// <inheritdoc />
    /// <inheritdoc />
    public override string Name => Key;

    /// <inheritdoc />
    public override string DisplayName => "End of month";

    /// <inheritdoc />
    public override string Description => "Last moment of the current calendar month.";

    protected override DateTime Input
    {
        get
        {
            var today = DateTime.UtcNow;
            var start = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1).AddTicks(-1);
            return end;
        }
    }
}
