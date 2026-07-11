namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
///     Base exception for query engine failures that should be mapped to HTTP responses.
/// </summary>
public class QueryException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="QueryException" /> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <param name="reason">Stable machine-oriented category for this failure type (constant per derived exception type).</param>
    /// <param name="message">Human-readable description of this specific failure.</param>
    public QueryException(int statusCode, string reason, string message)
        : base(message)
    {
        StatusCode = statusCode;
        Reason = reason;
    }

    /// <summary>
    ///     Initializes a new instance with an inner exception for diagnostics.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <param name="reason">Stable machine-oriented category for this failure type.</param>
    /// <param name="message">Human-readable description of this specific failure.</param>
    /// <param name="innerException">The underlying error.</param>
    public QueryException(int statusCode, string reason, string message, Exception? innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Reason = reason;
    }

    /// <summary>
    ///     Gets the HTTP status code associated with this failure.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    ///     Gets the stable failure category for this exception type (for example a constant exposed on
    ///     <see cref="QueryValidationException" />).
    /// </summary>
    public string Reason { get; }
}