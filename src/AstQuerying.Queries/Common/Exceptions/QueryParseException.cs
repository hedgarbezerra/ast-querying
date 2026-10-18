using Microsoft.AspNetCore.Http;

namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
///     Thrown when a filter clause cannot be parsed into a valid AST.
/// </summary>
public sealed class QueryParseException : QueryException
{
    /// <summary>
    ///     Stable <see cref="QueryException.Reason" /> value for all instances of this exception type.
    /// </summary>
    public const string ReasonCode = "QueryParse";

    /// <summary>
    ///     Initializes a new instance with HTTP 400.
    /// </summary>
    /// <param name="message">Description of the parse failure.</param>
    /// <param name="clauseKind">Optional categorization of the invalid clause.</param>
    public QueryParseException(string message, ClauseKind? clauseKind = null)
        : base(StatusCodes.Status400BadRequest, ReasonCode, message)
    {
        ClauseKind = clauseKind;
    }

    /// <summary>
    ///     Gets the optional clause failure category.
    /// </summary>
    public ClauseKind? ClauseKind { get; }

    /// <inheritdoc />
    protected override void WriteExtensions(IDictionary<string, object?> extensions)
    {
        if (ClauseKind is not null)
            extensions["clauseKind"] = ClauseKind.Value.ToString();
    }
}

/// <summary>
///     Categorizes the kind of filter clause parse failure.
/// </summary>
public enum ClauseKind
{
    /// <summary>
    ///     The clause structure is invalid.
    /// </summary>
    InvalidClause,

    /// <summary>
    ///     A leaf comparison is missing its operator.
    /// </summary>
    MissingOperator,

    /// <summary>
    ///     A leaf comparison is missing its value.
    /// </summary>
    MissingValue,

    /// <summary>
    ///     The AST node type is not recognized.
    /// </summary>
    UnknownNode,

    /// <summary>
    ///     The value node type is not supported.
    /// </summary>
    UnsupportedValueNode
}
