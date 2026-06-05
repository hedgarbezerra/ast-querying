using System.Reflection;

namespace AstQuerying.Queries.Configuration.DependencyInjection;

/// <summary>
/// Options controlling assembly scanning for query configurations and numeric limits for pagination.
/// </summary>
public sealed class QueryEngineOptions
{
    /// <summary>
    /// Gets assemblies scanned for <see cref="T:AstQuerying.Queries.Configuration.QueryConfiguration`1"/> types and optional plugin operators/functions.
    /// </summary>
    public IList<Assembly> AssembliesToScan { get; } = new List<Assembly>();

    /// <summary>
    /// Gets or sets the maximum allowed skip offset (for both explicit skip and page-derived skip).
    /// </summary>
    public int MaxSkip { get; set; } = 50_000;

    /// <summary>
    /// Gets or sets the maximum allowed take size when using skip/take pagination.
    /// </summary>
    public int MaxTake { get; set; } = 500;

    /// <summary>
    /// Gets or sets the maximum allowed page size when using page/pageSize pagination.
    /// </summary>
    public int MaxPageSize { get; set; } = 500;

    /// <summary>
    /// Gets or sets the default take when the client omits take.
    /// </summary>
    public int DefaultTake { get; set; } = 50;

    /// <summary>
    /// Gets or sets the default page size when the client omits pageSize.
    /// </summary>
    public int DefaultPageSize { get; set; } = 50;
}
