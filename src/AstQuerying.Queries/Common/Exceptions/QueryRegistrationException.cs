using Microsoft.AspNetCore.Http;

namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
///     Thrown when operator or function registration fails at startup.
/// </summary>
public sealed class QueryRegistrationException : QueryException
{
    /// <summary>
    ///     Stable <see cref="QueryException.Reason" /> value for all instances of this exception type.
    /// </summary>
    public const string ReasonCode = "QueryRegistration";

    /// <summary>
    ///     Initializes a new instance with HTTP 500.
    /// </summary>
    /// <param name="duplicateName">The conflicting registration name.</param>
    /// <param name="kind">Whether an operator or function registration failed.</param>
    /// <param name="message">Human-readable description.</param>
    public QueryRegistrationException(string duplicateName, RegistrationKind kind, string message)
        : base(StatusCodes.Status500InternalServerError, ReasonCode, message)
    {
        DuplicateName = duplicateName;
        RegistrationKind = kind;
    }

    /// <summary>
    ///     Gets the conflicting registration name.
    /// </summary>
    public string DuplicateName { get; }

    /// <summary>
    ///     Gets whether an operator or function registration failed.
    /// </summary>
    public RegistrationKind RegistrationKind { get; }

    /// <inheritdoc />
    protected override void WriteExtensions(IDictionary<string, object?> extensions)
    {
        extensions["duplicateName"] = DuplicateName;
        extensions["registrationKind"] = RegistrationKind.ToString();
    }
}

/// <summary>
///     Categorizes registration failures.
/// </summary>
public enum RegistrationKind
{
    /// <summary>
    ///     A filter operator registration conflict.
    /// </summary>
    Operator,

    /// <summary>
    ///     A query function registration conflict.
    /// </summary>
    Function
}
