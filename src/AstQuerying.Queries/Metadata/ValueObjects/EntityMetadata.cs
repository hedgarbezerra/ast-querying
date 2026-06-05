using AstQuerying.Queries.Filters.ValueObjects;
using AstQuerying.Queries.Sorting.ValueObjects;

namespace AstQuerying.Queries.Metadata.ValueObjects;

/// <summary>
/// Immutable metadata for a single entity type, including all filterable/sortable properties and optional defaults.
/// </summary>
public sealed class EntityMetadata
{
    /// <summary>
    /// Initializes a new instance of <see cref="EntityMetadata"/>.
    /// </summary>
    /// <param name="clrType">The entity CLR type.</param>
    /// <param name="properties">Configured property rules.</param>
    /// <param name="defaultSort">Optional default sort applied when the client sends none.</param>
    /// <param name="defaultFilter">Optional default filter merged or applied by the host application.</param>
    public EntityMetadata(
        Type clrType,
        IReadOnlyList<PropertyMetadata> properties,
        IReadOnlyList<SortFieldDto>? defaultSort = null,
        FilterClauseDto? defaultFilter = null)
    {
        ClrType = clrType;
        Properties = properties;
        DefaultSort = defaultSort;
        DefaultFilter = defaultFilter;
    }

    /// <summary>
    /// Gets the entity CLR type this metadata describes.
    /// </summary>
    public Type ClrType { get; }

    /// <summary>
    /// Gets all configured properties for filtering and sorting.
    /// </summary>
    public IReadOnlyList<PropertyMetadata> Properties { get; }

    /// <summary>
    /// Gets the configured default sort fields, if any.
    /// </summary>
    public IReadOnlyList<SortFieldDto>? DefaultSort { get; }

    /// <summary>
    /// Gets the configured default filter clause, if any.
    /// </summary>
    public FilterClauseDto? DefaultFilter { get; }

    /// <summary>
    /// Finds metadata by exact path or configured alias (case-insensitive).
    /// </summary>
    /// <param name="key">The path or alias from the client.</param>
    /// <returns>The matching <see cref="PropertyMetadata"/>, or <see langword="null"/>.</returns>
    public PropertyMetadata? FindByPathOrAlias(string key)
    {
        foreach (var p in Properties)
        {
            if (string.Equals(p.Path, key, StringComparison.OrdinalIgnoreCase))
                return p;
            if (p.Alias is not null && string.Equals(p.Alias, key, StringComparison.OrdinalIgnoreCase))
                return p;
        }

        return null;
    }
}
