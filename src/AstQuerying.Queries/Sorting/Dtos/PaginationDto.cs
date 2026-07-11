using System.Text.Json.Serialization;

namespace AstQuerying.Queries.Sorting.Dtos;

/// <summary>
///     Client pagination input using one-based <c>page</c> and <c>size</c>.
/// </summary>
public sealed class PaginationDto
{
    /// <summary>
    ///     Gets or sets the one-based page index. When omitted, defaults to 1.
    /// </summary>
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    /// <summary>
    ///     Gets or sets the page size. When omitted, the configured default page size applies.
    /// </summary>
    [JsonPropertyName("size")]
    public int? Size { get; set; }
}