using System.Text.Json;
using System.Text.Json.Serialization;

namespace AstQuerying.Queries.Filters.ValueObjects;

/// <summary>
/// JSON-friendly representation of a filter clause tree (logical nodes and leaf comparisons).
/// </summary>
public sealed class FilterClauseDto
{
    /// <summary>
    /// Gets or sets child clauses combined with logical AND.
    /// </summary>
    [JsonPropertyName("and")]
    public List<FilterClauseDto>? And { get; set; }

    /// <summary>
    /// Gets or sets child clauses combined with logical OR.
    /// </summary>
    [JsonPropertyName("or")]
    public List<FilterClauseDto>? Or { get; set; }

    /// <summary>
    /// Gets or sets a single clause wrapped with logical NOT.
    /// </summary>
    [JsonPropertyName("not")]
    public FilterClauseDto? Not { get; set; }

    /// <summary>
    /// Gets or sets the property path or alias for a leaf comparison.
    /// </summary>
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    /// <summary>
    /// Gets or sets the operator name (long form).
    /// </summary>
    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    /// <summary>
    /// Gets or sets the operator short alias (for example <c>op</c>).
    /// </summary>
    [JsonPropertyName("op")]
    public string? Op { get; set; }

    /// <summary>
    /// Gets or sets the single comparison value for unary operators.
    /// </summary>
    [JsonPropertyName("value")]
    public JsonElement? Value { get; set; }

    /// <summary>
    /// Gets or sets multiple values for set-style operators such as <c>in</c>.
    /// </summary>
    [JsonPropertyName("values")]
    public List<JsonElement>? Values { get; set; }

    /// <summary>
    /// Gets the effective operator token, preferring <see cref="Operator"/> and falling back to <see cref="Op"/>.
    /// </summary>
    [JsonIgnore]
    public string? ResolvedOperator => Operator ?? Op;
}
