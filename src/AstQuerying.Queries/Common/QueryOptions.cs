namespace AstQuerying.Queries.Common;

/// <summary>
///     Options controlling numeric limits for pagination.
/// </summary>
public sealed class QueryOptions
{
    /// <summary>
    ///     Gets or sets the maximum allowed zero-based skip derived from <c>(page - 1) * size</c>.
    /// </summary>
    public int MaxSkip { get; set; } = 50_000;

    /// <summary>
    ///     Gets or sets the maximum allowed page size (maps to LINQ <c>Take</c>).
    /// </summary>
    public int MaxPageSize { get; set; } = 500;

    /// <summary>
    ///     Gets or sets the default page size when the client omits <c>size</c>.
    /// </summary>
    public int DefaultPageSize { get; set; } = 50;
}
