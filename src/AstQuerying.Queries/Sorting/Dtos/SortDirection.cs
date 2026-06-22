using System.Text.Json.Serialization;

namespace AstQuerying.Queries.Sorting.Dtos;

/// <summary>
/// Specifies sort order for a field in API payloads.
/// </summary>
public enum SortDirection
{
    /// <summary>
    /// Descending order; JSON value <c>desc</c>.
    /// </summary>
    [JsonPropertyName("desc")]
    Descending = 0,

    /// <summary>
    /// Ascending order; JSON value <c>asc</c>.
    /// </summary>
    [JsonPropertyName("asc")]
    Ascending = 1
}
