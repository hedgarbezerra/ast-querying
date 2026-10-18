using Microsoft.AspNetCore.Http;

namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
///     Thrown when a property path cannot be resolved for the requested operation.
/// </summary>
public sealed class QueryUnsupportedPathException : QueryException
{
    /// <summary>
    ///     Stable <see cref="QueryException.Reason" /> value for all instances of this exception type.
    /// </summary>
    public const string ReasonCode = "QueryUnsupportedPath";

    /// <summary>
    ///     Initializes a new instance with HTTP 400.
    /// </summary>
    /// <param name="entityType">The entity CLR type.</param>
    /// <param name="fieldPath">The unsupported field path.</param>
    /// <param name="reason">A short reason code or description.</param>
    public QueryUnsupportedPathException(Type entityType, string fieldPath, string reason)
        : base(StatusCodes.Status400BadRequest, ReasonCode,
            $"Property path '{fieldPath}' is not supported for entity {entityType.Name}: {reason}.")
    {
        EntityType = entityType;
        FieldPath = fieldPath;
        ReasonDetail = reason;
    }

    /// <summary>
    ///     Gets the entity CLR type.
    /// </summary>
    public Type EntityType { get; }

    /// <summary>
    ///     Gets the unsupported field path.
    /// </summary>
    public string FieldPath { get; }

    /// <summary>
    ///     Gets a short reason for the unsupported path.
    /// </summary>
    public string ReasonDetail { get; }

    /// <inheritdoc />
    protected override void WriteExtensions(IDictionary<string, object?> extensions)
    {
        extensions["entityType"] = EntityType.FullName ?? EntityType.Name;
        extensions["fieldPath"] = FieldPath;
        extensions["pathReason"] = ReasonDetail;
    }
}
