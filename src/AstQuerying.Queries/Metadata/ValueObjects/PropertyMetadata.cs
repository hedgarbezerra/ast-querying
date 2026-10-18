namespace AstQuerying.Queries.Metadata.ValueObjects;

/// <summary>
///     Describes a single filterable/sortable property path on an entity.
/// </summary>
public sealed class PropertyMetadata
{
    /// <summary>
    ///     Initializes a new instance of <see cref="PropertyMetadata" />.
    /// </summary>
    /// <param name="path">The dotted path from the entity root.</param>
    /// <param name="leafType">The CLR type at the leaf of the path.</param>
    /// <param name="segments">The ordered path segments.</param>
    public PropertyMetadata(
        string path,
        Type leafType,
        IReadOnlyList<PathSegment> segments)
    {
        Path = path;
        LeafType = leafType;
        Segments = segments;
    }

    /// <summary>
    ///     Gets the dotted path from the entity root.
    /// </summary>
    public string Path { get; }

    /// <summary>
    ///     Gets the CLR leaf type used for conversions and operators.
    /// </summary>
    public Type LeafType { get; }

    /// <summary>
    ///     Gets the ordered segments describing navigation from the entity root to the leaf.
    /// </summary>
    public IReadOnlyList<PathSegment> Segments { get; }
}
