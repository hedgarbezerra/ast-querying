namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
/// Base exception for query engine failures that should be mapped to HTTP responses.
/// </summary>
public class QueryException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryException"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <param name="reason">A short, user-safe explanation.</param>
    public QueryException(int statusCode, string reason)
        : base(reason)
    {
        StatusCode = statusCode;
        Reason = reason;
    }

    /// <summary>
    /// Initializes a new instance with an inner exception for diagnostics.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <param name="reason">A short, user-safe explanation.</param>
    /// <param name="innerException">The underlying error.</param>
    public QueryException(int statusCode, string reason, Exception? innerException)
        : base(reason, innerException)
    {
        StatusCode = statusCode;
        Reason = reason;
    }

    /// <summary>
    /// Gets the HTTP status code associated with this failure.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Gets the human-readable reason (duplicated from <see cref="Exception.Message"/> for clarity in APIs).
    /// </summary>
    public string Reason { get; }
}
