using AstQuerying.Queries.Metadata.ValueObjects;

namespace AstQuerying.Queries.Metadata.Contracts;

/// <summary>
/// Resolves <see cref="EntityMetadata"/> for arbitrary entity CLR types at runtime.
/// </summary>
public interface IQueryMetadataProvider
{
    /// <summary>
    /// Gets metadata for the given entity type, or <see langword="null"/> if it is not registered.
    /// </summary>
    /// <param name="entityClrType">The CLR type of the entity.</param>
    EntityMetadata? GetMetadata(Type entityClrType);
}

/// <summary>
/// Provides strongly typed access to metadata for a single entity <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The entity CLR type.</typeparam>
public interface IQueryMetadataProvider<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets metadata for <typeparamref name="TEntity"/>; throws when the entity is not registered.
    /// </summary>
    EntityMetadata GetMetadataForEntity();
}
