using AstQuerying.Queries.Metadata.Contracts;
using AstQuerying.Queries.Metadata.ValueObjects;
using AstQuerying.Queries.Registry.Contracts;

namespace AstQuerying.Queries.Metadata.Implementations;

/// <summary>
/// Non-generic <see cref="IQueryMetadataProvider"/> that resolves metadata from <see cref="IQueryRegistry"/>.
/// </summary>
public sealed class RegistryQueryMetadataProvider : IQueryMetadataProvider
{
    private readonly IQueryRegistry _registry;

    /// <summary>
    /// Initializes a new instance of <see cref="RegistryQueryMetadataProvider"/>.
    /// </summary>
    /// <param name="registry">The query registry.</param>
    public RegistryQueryMetadataProvider(IQueryRegistry registry)
    {
        _registry = registry;
    }

    /// <inheritdoc />
    public EntityMetadata? GetMetadata(Type entityClrType)
    {
        return _registry.GetEntity(entityClrType);
    }
}
