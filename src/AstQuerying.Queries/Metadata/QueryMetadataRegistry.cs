using AstQuerying.Queries.Metadata.ValueObjects;

namespace AstQuerying.Queries.Metadata;

/// <summary>
///     Static registry populated at module initialization by source-generated metadata.
/// </summary>
public static class QueryMetadataRegistry
{
    private static readonly Dictionary<Type, EntityMetadata> Entries = new();

    /// <summary>
    ///     Registers metadata for an entity type. Called by generated <c>ModuleInitializer</c> code.
    /// </summary>
    /// <param name="entityType">The entity CLR type.</param>
    /// <param name="metadata">The generated entity metadata.</param>
    public static void Register(Type entityType, EntityMetadata metadata)
    {
        lock (Entries)
        {
            Entries[entityType] = metadata;
        }
    }

    /// <summary>
    ///     Gets metadata for an entity type, or <see langword="null" /> when not registered.
    /// </summary>
    /// <param name="entityType">The entity CLR type.</param>
    /// <returns>The entity metadata when found.</returns>
    internal static EntityMetadata? Get(Type entityType)
    {
        lock (Entries)
        {
            return Entries.TryGetValue(entityType, out var meta) ? meta : null;
        }
    }

    /// <summary>
    ///     Gets all registered entity metadata entries.
    /// </summary>
    internal static IReadOnlyDictionary<Type, EntityMetadata> All
    {
        get
        {
            lock (Entries)
            {
                return new Dictionary<Type, EntityMetadata>(Entries);
            }
        }
    }
}
