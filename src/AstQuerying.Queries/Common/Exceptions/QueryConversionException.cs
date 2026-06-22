using Microsoft.AspNetCore.Http;

namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
/// Thrown when a literal or function result cannot be converted to the target property type.
/// </summary>
public sealed class QueryConversionException : QueryException
{
    /// <summary>
    /// Stable <see cref="QueryException.Reason"/> value for all instances of this exception type.
    /// </summary>
    public const string ReasonCode = "QueryConversion";

    /// <summary>
    /// Initializes a new instance with HTTP 400.
    /// </summary>
    /// <param name="message">Description of the conversion failure.</param>
    public QueryConversionException(string message)
        : base(StatusCodes.Status400BadRequest, ReasonCode, message)
    {
    }

    /// <summary>
    /// Initializes a new instance with an inner exception.
    /// </summary>
    /// <param name="message">Description of the conversion failure.</param>
    /// <param name="innerException">The underlying error.</param>
    public QueryConversionException(string message, Exception? innerException)
        : base(StatusCodes.Status400BadRequest, ReasonCode, message, innerException)
    {
    }
}
