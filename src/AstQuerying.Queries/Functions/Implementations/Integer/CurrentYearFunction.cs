using AstQuerying.Queries.Functions.Contracts;

namespace AstQuerying.Queries.Functions.Implementations.Integer;

/// <summary>
/// Built-in provider for the current calendar year as an integer.
/// </summary>
public sealed class CurrentYearFunction : BaseIntegerFunction
{
    public const string Key = "CurrentYear";

    /// <inheritdoc />
    /// <inheritdoc />
    public override string Name => Key;

    /// <inheritdoc />
    public override string DisplayName => "Current year";

    /// <inheritdoc />
    public override string Description => "Current calendar year.";

    /// <inheritdoc />
    protected override int Input => DateTime.Now.Year;
}
