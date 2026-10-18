using Microsoft.AspNetCore.Http;

namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
///     Thrown when an entity type has no generated query metadata.
/// </summary>
public sealed class QueryEntityNotQueryableException : QueryException
{
    /// <summary>
    ///     Stable <see cref="QueryException.Reason" /> value for all instances of this exception type.
    /// </summary>
    public const string ReasonCode = "QueryEntityNotQueryable";

    /// <summary>
    ///     Initializes a new instance with HTTP 400.
    /// </summary>
    /// <param name="entityType">The entity CLR type without metadata.</param>
    public QueryEntityNotQueryableException(Type entityType)
        : base(StatusCodes.Status400BadRequest, ReasonCode,
            $"No query metadata registered for entity {entityType.Name}.")
    {
        EntityType = entityType;
    }

    /// <summary>
    ///     Gets the entity CLR type.
    /// </summary>
    public Type EntityType { get; }

    /// <inheritdoc />
    protected override void WriteExtensions(IDictionary<string, object?> extensions)
    {
        extensions["entityType"] = EntityType.FullName ?? EntityType.Name;
    }
}
