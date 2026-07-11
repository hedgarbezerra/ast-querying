namespace AstQuerying.Queries.Functions.Monetary;

/// <summary>
///     Built-in provider for a fixed minimum wage amount used in policy filters.
/// </summary>
public sealed class MinimumWageFunction : BaseMonetaryFunction
{
    private const decimal DefaultAmount = 1500.00m;

    public const string Key = "MinimumWage";

    /// <inheritdoc />
    public override string Name => Key;

    /// <inheritdoc />
    public override string DisplayName => "Minimum wage";

    /// <inheritdoc />
    public override string Description => "Fixed default minimum wage amount for policy comparisons.";

    /// <inheritdoc />
    protected override decimal Input => DefaultAmount;
}