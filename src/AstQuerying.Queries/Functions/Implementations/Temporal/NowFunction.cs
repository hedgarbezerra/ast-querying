using AstQuerying.Queries.Functions.Contracts;

namespace AstQuerying.Queries.Functions.Implementations.Temporal;

/// <summary>
/// Built-in provider for the current UTC date and time.
/// </summary>
public sealed class NowFunction : BaseTemporalFunction
{
    public const string Key = "Now";

    /// <inheritdoc />
    public override string Name => Key;

    /// <inheritdoc />
    public override string DisplayName => "Now";

    /// <inheritdoc />
    public override string Description => "Current UTC date and time.";

    /// <inheritdoc />
    protected override DateTime Input => DateTime.UtcNow;
}
