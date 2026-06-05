namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
/// Thrown when entity query rules are misconfigured at startup (duplicate paths, invalid defaults, and similar).
/// </summary>
public sealed class QueryConfigurationException : Exception
{
    /// <summary>
    /// Initializes a new instance with a configuration error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public QueryConfigurationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The underlying error.</param>
    public QueryConfigurationException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
