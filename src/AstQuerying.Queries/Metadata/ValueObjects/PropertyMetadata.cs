namespace AstQuerying.Queries.Metadata.ValueObjects;

/// <summary>
/// Describes a single filterable/sortable property path on an entity, including optional constraints.
/// </summary>
public sealed class PropertyMetadata
{
    /// <summary>
    /// Initializes a new instance of <see cref="PropertyMetadata"/>.
    /// </summary>
    /// <param name="path">The dotted path from the entity root.</param>
    /// <param name="alias">Optional API alias for the path.</param>
    /// <param name="leafType">The CLR type at the leaf of the path.</param>
    /// <param name="segments">The ordered path segments.</param>
    /// <param name="defaultValue">Optional default literal used when the client omits a value.</param>
    /// <param name="allowedOperators">When set, only these operator names are accepted for this field.</param>
    /// <param name="allowedFunctions">When set, only these function names are accepted in operands.</param>
    /// <param name="allowSortAscending">Whether ascending sort is allowed.</param>
    /// <param name="allowSortDescending">Whether descending sort is allowed.</param>
    public PropertyMetadata(
        string path,
        string? alias,
        Type leafType,
        IReadOnlyList<PathSegment> segments,
        object? defaultValue,
        IReadOnlySet<string>? allowedOperators = null,
        IReadOnlySet<string>? allowedFunctions = null,
        bool allowSortAscending = false,
        bool allowSortDescending = false)
    {
        Path = path;
        Alias = alias;
        LeafType = leafType;
        Segments = segments;
        DefaultValue = defaultValue;
        AllowedOperators = allowedOperators;
        AllowedFunctions = allowedFunctions;
        AllowSortAscending = allowSortAscending;
        AllowSortDescending = allowSortDescending;
    }

    /// <summary>
    /// Gets the dotted path from the entity root.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the optional API alias.
    /// </summary>
    public string? Alias { get; }

    /// <summary>
    /// Gets the CLR leaf type used for conversions and operators.
    /// </summary>
    public Type LeafType { get; }

    /// <summary>
    /// Gets the ordered segments describing navigation from the entity root to the leaf.
    /// </summary>
    public IReadOnlyList<PathSegment> Segments { get; }

    /// <summary>
    /// Gets the optional default literal for this property.
    /// </summary>
    public object? DefaultValue { get; }

    /// <summary>
    /// Gets the allow-list of operator names, or <see langword="null"/> when all registered operators are allowed.
    /// </summary>
    public IReadOnlySet<string>? AllowedOperators { get; }

    /// <summary>
    /// Gets the allow-list of function names for dynamic operands, or <see langword="null"/> when all are allowed.
    /// </summary>
    public IReadOnlySet<string>? AllowedFunctions { get; }

    /// <summary>
    /// Gets a value indicating whether ascending sort is permitted.
    /// </summary>
    public bool AllowSortAscending { get; }

    /// <summary>
    /// Gets a value indicating whether descending sort is permitted.
    /// </summary>
    public bool AllowSortDescending { get; }
}
