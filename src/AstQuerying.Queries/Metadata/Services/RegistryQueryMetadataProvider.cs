using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Metadata.ValueObjects;
using AstQuerying.Queries.Registries;

namespace AstQuerying.Queries.Metadata.Services;

/// <summary>
///     <see cref="IQueryMetadataProvider" /> that resolves metadata from <see cref="IQueryRegistry" />.
/// </summary>
public sealed class RegistryQueryMetadataProvider : IQueryMetadataProvider
{
    private readonly IQueryRegistry _registry;

    /// <summary>
    ///     Initializes a new instance of <see cref="RegistryQueryMetadataProvider" />.
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

    /// <inheritdoc />
    public EntityMetadata GetMetadataForEntity<TEntity>() where TEntity : class
    {
        return _registry.GetEntity(typeof(TEntity)) ??
               throw new QueryValidationException($"No query metadata for {typeof(TEntity).Name}.");
    }
}