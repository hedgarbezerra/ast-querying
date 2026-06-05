using System.Text.Json.Serialization;

namespace AstQuerying.Queries.Sorting.ValueObjects;

/// <summary>
/// Client pagination input supporting either offset/limit (<c>skip</c>/<c>take</c>) or one-based page indexing.
/// </summary>
public sealed class PaginationRequestDto
{
    /// <summary>
    /// Gets or sets the zero-based row offset when using skip/take mode.
    /// </summary>
    [JsonPropertyName("skip")]
    public int? Skip { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of rows when using skip/take mode.
    /// </summary>
    [JsonPropertyName("take")]
    public int? Take { get; set; }

    /// <summary>
    /// Gets or sets the one-based page index when using page mode.
    /// </summary>
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    /// <summary>
    /// Gets or sets the page size when using page mode.
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int? PageSize { get; set; }
}
