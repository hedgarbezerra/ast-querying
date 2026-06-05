namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
/// Thrown when a literal or function result cannot be converted to the target property type.
/// </summary>
public sealed class QueryConversionException : QueryException
{
    /// <summary>
    /// Initializes a new instance with HTTP 400.
    /// </summary>
    /// <param name="reason">Description of the conversion failure.</param>
    public QueryConversionException(string reason)
        : base(400, reason)
    {
    }

    /// <summary>
    /// Initializes a new instance with an inner exception.
    /// </summary>
    /// <param name="reason">Description of the conversion failure.</param>
    /// <param name="innerException">The underlying error.</param>
    public QueryConversionException(string reason, Exception? innerException)
        : base(400, reason, innerException)
    {
    }
}
