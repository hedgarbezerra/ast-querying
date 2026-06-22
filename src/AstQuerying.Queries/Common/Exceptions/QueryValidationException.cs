using Microsoft.AspNetCore.Http;

namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
/// Thrown when a query payload or AST is structurally invalid or violates configured rules.
/// </summary>
public sealed class QueryValidationException : QueryException
{
    /// <summary>
    /// Stable <see cref="QueryException.Reason"/> value for all instances of this exception type.
    /// </summary>
    public const string ReasonCode = "QueryValidation";

    /// <summary>
    /// Initializes a new instance with HTTP 400 (Bad Request).
    /// </summary>
    /// <param name="message">Description of the validation failure.</param>
    public QueryValidationException(string message)
        : base(StatusCodes.Status400BadRequest, ReasonCode, message)
    {
    }

    /// <summary>
    /// Initializes a new instance with an inner exception.
    /// </summary>
    /// <param name="message">Description of the validation failure.</param>
    /// <param name="innerException">The underlying error.</param>
    public QueryValidationException(string message, Exception? innerException)
        : base(StatusCodes.Status400BadRequest, ReasonCode, message, innerException)
    {
    }
}
