using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Metadata.ValueObjects;

namespace AstQuerying.Queries.Metadata.Services;

/// <summary>
///     <see cref="IEntityMetadataProvider" /> that resolves metadata from <see cref="QueryMetadataRegistry" />.
/// </summary>
public sealed class GeneratedEntityMetadataProvider : IEntityMetadataProvider
{
    /// <inheritdoc />
    public EntityMetadata? GetMetadata(Type entityClrType)
    {
        return QueryMetadataRegistry.Get(entityClrType);
    }

    /// <inheritdoc />
    public EntityMetadata GetMetadataForEntity<TEntity>() where TEntity : class
    {
        return QueryMetadataRegistry.Get(typeof(TEntity)) ??
               throw new QueryEntityNotQueryableException(typeof(TEntity));
    }
}
