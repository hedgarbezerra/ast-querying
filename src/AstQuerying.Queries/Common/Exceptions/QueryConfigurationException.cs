using Microsoft.AspNetCore.Http;

namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
///     Thrown when entity query rules are misconfigured at startup (duplicate paths, invalid defaults, and similar).
/// </summary>
public sealed class QueryConfigurationException : QueryException
{
    /// <summary>
    ///     Stable <see cref="QueryException.Reason" /> value for all instances of this exception type.
    /// </summary>
    public const string ReasonCode = "QueryConfiguration";

    /// <summary>
    ///     Initializes a new instance with HTTP 500 (Internal Server Error) when mapped to a response.
    /// </summary>
    /// <param name="message">The configuration error description.</param>
    /// <param name="diagnostics">Optional structured context.</param>
    public QueryConfigurationException(string message, QueryConfigurationDiagnostics? diagnostics = null)
        : base(StatusCodes.Status500InternalServerError, ReasonCode, message)
    {
        Diagnostics = diagnostics;
    }

    /// <summary>
    ///     Initializes a new instance with an inner exception.
    /// </summary>
    /// <param name="message">The configuration error description.</param>
    /// <param name="innerException">The underlying error.</param>
    /// <param name="diagnostics">Optional structured context.</param>
    public QueryConfigurationException(string message, Exception? innerException,
        QueryConfigurationDiagnostics? diagnostics = null)
        : base(StatusCodes.Status500InternalServerError, ReasonCode, message, innerException)
    {
        Diagnostics = diagnostics;
    }

    /// <summary>
    ///     Gets structured context for this configuration failure, when supplied at construction.
    /// </summary>
    public QueryConfigurationDiagnostics? Diagnostics { get; }
}