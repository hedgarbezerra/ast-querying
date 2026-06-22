using AstQuerying.Queries.Functions.Contracts;

namespace AstQuerying.Queries.Functions.Implementations.Integer;

/// <summary>
/// Built-in provider for a fixed legal age threshold used in policy filters.
/// </summary>
public sealed class LegalAgeFunction : BaseIntegerFunction
{
    private const int LegalAgeYears = 18;

    public const string Key = "LegalAge";

    /// <inheritdoc />
    /// <inheritdoc />
    public override string Name => Key;

    /// <inheritdoc />
    public override string DisplayName => "Legal age";

    /// <inheritdoc />
    public override string Description => "Fixed legal age threshold (18 years) for policy comparisons.";

    /// <inheritdoc />
    protected override int Input => LegalAgeYears;
}
