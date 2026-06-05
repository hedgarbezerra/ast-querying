using System.Text.Json.Serialization;

namespace AstQuerying.Queries.Sorting.ValueObjects;

/// <summary>
/// Describes a single sort field and direction in API payloads.
/// </summary>
public sealed class SortFieldDto
{
    /// <summary>
    /// Gets or sets the property path or alias to sort by.
    /// </summary>
    [JsonPropertyName("field")]
    public string Field { get; set; } = "";

    /// <summary>
    /// Gets or sets the sort direction; typically <c>asc</c> or <c>desc</c> (case-insensitive in callers).
    /// </summary>
    [JsonPropertyName("direction")]
    public string Direction { get; set; } = "desc";
}
