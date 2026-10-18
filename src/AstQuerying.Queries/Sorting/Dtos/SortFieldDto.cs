using System.Text.Json.Serialization;

namespace AstQuerying.Queries.Sorting.Dtos;

/// <summary>
///     Describes a single sort field and direction in API payloads. In a sort list, index 0 maps to
///     <c>OrderBy</c> and later indices map to <c>ThenBy</c> in the same order.
/// </summary>
public sealed class SortFieldDto
{
    /// <summary>
    ///     Gets or sets the property path or alias to sort by.
    /// </summary>
    [JsonPropertyName("field")]
    public string Field { get; set; } = "";

    /// <summary>
    ///     Gets or sets the sort direction; JSON uses <c>asc</c> or <c>desc</c>.
    /// </summary>
    [JsonPropertyName("direction")]
    public SortDirection Direction { get; set; } = SortDirection.Descending;
}