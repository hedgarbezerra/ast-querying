using Microsoft.AspNetCore.Http;

namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
///     Thrown when a query references a function that is not registered.
/// </summary>
public sealed class QueryUnknownFunctionException : QueryException
{
    /// <summary>
    ///     Stable <see cref="QueryException.Reason" /> value for all instances of this exception type.
    /// </summary>
    public const string ReasonCode = "QueryUnknownFunction";

    /// <summary>
    ///     Initializes a new instance with HTTP 400.
    /// </summary>
    /// <param name="functionName">The unknown function name.</param>
    /// <param name="fieldPath">Optional field path context.</param>
    public QueryUnknownFunctionException(string functionName, string? fieldPath = null)
        : base(StatusCodes.Status400BadRequest, ReasonCode,
            fieldPath is null
                ? $"Unknown function '{functionName}'."
                : $"Unknown function '{functionName}' for field '{fieldPath}'.")
    {
        FunctionName = functionName;
        FieldPath = fieldPath;
    }

    /// <summary>
    ///     Gets the unknown function name.
    /// </summary>
    public string FunctionName { get; }

    /// <summary>
    ///     Gets the optional field path context.
    /// </summary>
    public string? FieldPath { get; }

    /// <inheritdoc />
    protected override void WriteExtensions(IDictionary<string, object?> extensions)
    {
        extensions["functionName"] = FunctionName;
        if (FieldPath is not null)
            extensions["fieldPath"] = FieldPath;
    }
}
