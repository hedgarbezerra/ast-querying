using System.Linq.Expressions;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Metadata;

namespace AstQuerying.Queries.Resolving.Services;

/// <summary>
///     Resolves configured paths to member-access expressions for a registered entity type.
/// </summary>
public sealed class PropertyResolver : IPropertyResolver
{
    private readonly IEntityMetadataProvider _metadata;

    /// <summary>
    ///     Initializes a new instance of <see cref="PropertyResolver" />.
    /// </summary>
    /// <param name="metadata">The entity metadata provider.</param>
    public PropertyResolver(IEntityMetadataProvider metadata)
    {
        _metadata = metadata;
    }

    /// <inheritdoc />
    public Expression Resolve<TEntity>(string path) where TEntity : class
    {
        var entityType = typeof(TEntity);
        var meta = _metadata.GetMetadata(entityType) ??
                   throw new QueryEntityNotQueryableException(entityType);

        var prop = meta.FindByPath(path) ??
                   throw new QueryUnknownFieldException(entityType, path);

        if (prop.Segments.Any(s => s.IsCollection))
            throw new QueryUnsupportedPathException(entityType, prop.Path, "collection paths are not supported");

        var param = Expression.Parameter(typeof(TEntity), "e");
        Expression cur = param;
        foreach (var seg in prop.Segments)
            cur = Expression.PropertyOrField(cur, seg.Name);

        return cur;
    }
}
