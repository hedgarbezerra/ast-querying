using Microsoft.AspNetCore.Http;

namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
///     Thrown when a query references a field path that is not present in entity metadata.
/// </summary>
public sealed class QueryUnknownFieldException : QueryException
{
    /// <summary>
    ///     Stable <see cref="QueryException.Reason" /> value for all instances of this exception type.
    /// </summary>
    public const string ReasonCode = "QueryUnknownField";

    /// <summary>
    ///     Initializes a new instance with HTTP 400.
    /// </summary>
    /// <param name="entityType">The entity CLR type.</param>
    /// <param name="fieldPath">The unknown field path.</param>
    public QueryUnknownFieldException(Type entityType, string fieldPath)
        : base(StatusCodes.Status400BadRequest, ReasonCode,
            $"Unknown field '{fieldPath}' for entity {entityType.Name}.")
    {
        EntityType = entityType;
        FieldPath = fieldPath;
    }

    /// <summary>
    ///     Gets the entity CLR type.
    /// </summary>
    public Type EntityType { get; }

    /// <summary>
    ///     Gets the unknown field path from the client.
    /// </summary>
    public string FieldPath { get; }

    /// <inheritdoc />
    protected override void WriteExtensions(IDictionary<string, object?> extensions)
    {
        extensions["entityType"] = EntityType.FullName ?? EntityType.Name;
        extensions["fieldPath"] = FieldPath;
    }
}
