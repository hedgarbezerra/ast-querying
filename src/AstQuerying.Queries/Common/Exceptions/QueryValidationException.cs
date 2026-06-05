using Microsoft.AspNetCore.Http;

namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
/// Thrown when a query payload or AST is structurally invalid or violates configured rules.
/// </summary>
public sealed class QueryValidationException : QueryException
{
    /// <summary>
    /// Initializes a new instance with HTTP 400 (Bad Request).
    /// </summary>
    /// <param name="reason">Description of the validation failure.</param>
    public QueryValidationException(string reason)
        : base(StatusCodes.Status400BadRequest, reason)
    {
    }

    /// <summary>
    /// Initializes a new instance with an inner exception.
    /// </summary>
    /// <param name="reason">Description of the validation failure.</param>
    /// <param name="innerException">The underlying error.</param>
    public QueryValidationException(string reason, Exception? innerException)
        : base(StatusCodes.Status400BadRequest, reason, innerException)
    {
    }
}
