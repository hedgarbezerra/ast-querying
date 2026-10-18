namespace AstQuerying.Queries.Filters;

/// <summary>
///     Filter operator registered in <see cref="T:AstQuerying.Queries.Registries.IOperatorRegistry" />.
/// </summary>
public interface IFilterOperator
{
    /// <summary>
    ///     Gets the API token for this operator (same value as the type&apos;s <c>public const string Key</c>).
    /// </summary>
    string Name { get; }
}