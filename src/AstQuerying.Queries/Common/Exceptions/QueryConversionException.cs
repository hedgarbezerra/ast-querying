using Microsoft.AspNetCore.Http;

namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
///     Thrown when a literal or function result cannot be converted to the target property type.
/// </summary>
public sealed class QueryConversionException : QueryException
{
    /// <summary>
    ///     Stable <see cref="QueryException.Reason" /> value for all instances of this exception type.
    /// </summary>
    public const string ReasonCode = "QueryConversion";

    /// <summary>
    ///     Initializes a new instance with HTTP 400.
    /// </summary>
    /// <param name="message">Description of the conversion failure.</param>
    /// <param name="targetType">The target CLR type.</param>
    /// <param name="sourceValue">The source value that could not be converted.</param>
    public QueryConversionException(string message, Type? targetType = null, object? sourceValue = null)
        : base(StatusCodes.Status400BadRequest, ReasonCode, message)
    {
        TargetType = targetType;
        SourceValue = sourceValue;
    }

    /// <summary>
    ///     Initializes a new instance with an inner exception.
    /// </summary>
    /// <param name="message">Description of the conversion failure.</param>
    /// <param name="innerException">The underlying error.</param>
    /// <param name="targetType">The target CLR type.</param>
    /// <param name="sourceValue">The source value that could not be converted.</param>
    public QueryConversionException(
        string message,
        Exception? innerException,
        Type? targetType = null,
        object? sourceValue = null)
        : base(StatusCodes.Status400BadRequest, ReasonCode, message, innerException)
    {
        TargetType = targetType;
        SourceValue = sourceValue;
    }

    /// <summary>
    ///     Gets the target CLR type.
    /// </summary>
    public Type? TargetType { get; }

    /// <summary>
    ///     Gets the source value that could not be converted.
    /// </summary>
    public object? SourceValue { get; }

    /// <inheritdoc />
    protected override void WriteExtensions(IDictionary<string, object?> extensions)
    {
        if (TargetType is not null)
            extensions["targetType"] = TargetType.FullName ?? TargetType.Name;
        if (SourceValue is not null)
            extensions["sourceValue"] = SourceValue;
    }
}
