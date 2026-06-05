using System.Linq.Expressions;
using AstQuerying.Queries.Common.Exceptions;
using AstQuerying.Queries.Metadata.ValueObjects;
using AstQuerying.Queries.Registry.Contracts;
using AstQuerying.Queries.Resolving.Contracts;

namespace AstQuerying.Queries.Resolving.Implementations;

/// <summary>
/// Resolves configured paths or aliases to member-access expressions for <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">The entity root type.</typeparam>
public sealed class PropertyResolver<TEntity> : IPropertyResolver<TEntity> where TEntity : class
{
    private readonly IQueryRegistry _registry;

    /// <summary>
    /// Initializes a new instance of <see cref="PropertyResolver{TEntity}"/>.
    /// </summary>
    /// <param name="registry">The query registry.</param>
    public PropertyResolver(IQueryRegistry registry)
    {
        _registry = registry;
    }

    /// <inheritdoc />
    public Expression Resolve(string pathOrAlias)
    {
        var meta = _registry.GetEntity(typeof(TEntity)) ??
                   throw new QueryValidationException($"No query metadata for {typeof(TEntity).Name}.");

        var prop = meta.FindByPathOrAlias(pathOrAlias) ??
                   throw new QueryValidationException($"Unknown field '{pathOrAlias}'.");

        if (prop.Segments.Any(s => s.IsCollection))
            throw new QueryValidationException($"Property resolver does not support collection path '{prop.Path}'.");

        var param = Expression.Parameter(typeof(TEntity), "e");
        Expression cur = param;
        foreach (var seg in prop.Segments)
            cur = Expression.PropertyOrField(cur, seg.Name);

        return cur;
    }
}
