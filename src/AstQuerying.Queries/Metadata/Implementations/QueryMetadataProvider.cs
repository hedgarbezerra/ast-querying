using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Metadata.Contracts;
using AstQuerying.Queries.Metadata.ValueObjects;
using AstQuerying.Queries.Registry.Contracts;

namespace AstQuerying.Queries.Metadata.Implementations;

/// <summary>
/// <see cref="IQueryMetadataProvider{TEntity}"/> implementation backed by <see cref="IQueryRegistry"/>.
/// </summary>
/// <typeparam name="TEntity">The entity root type.</typeparam>
public sealed class QueryMetadataProvider<TEntity> : IQueryMetadataProvider<TEntity> where TEntity : class
{
    private readonly IQueryRegistry _registry;

    /// <summary>
    /// Initializes a new instance of <see cref="QueryMetadataProvider{TEntity}"/>.
    /// </summary>
    /// <param name="registry">The query registry.</param>
    public QueryMetadataProvider(IQueryRegistry registry)
    {
        _registry = registry;
    }

    /// <inheritdoc />
    public EntityMetadata GetMetadataForEntity()
    {
        return _registry.GetEntity(typeof(TEntity)) ??
               throw new QueryValidationException($"No query metadata for {typeof(TEntity).Name}.");
    }
}
