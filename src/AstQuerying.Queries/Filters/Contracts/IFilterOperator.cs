namespace AstQuerying.Queries.Filters.Contracts;

/// <summary>
/// Filter operator registered in <see cref="T:AstQuerying.Queries.Registry.Contracts.IQueryRegistry.Operators"/>.
/// </summary>
public interface IFilterOperator
{
    /// <summary>
    /// Gets the API token for this operator (same value as the type&apos;s <c>public const string Key</c>).
    /// </summary>
    string Name { get; }
}
