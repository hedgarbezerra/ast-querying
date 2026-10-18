using Microsoft.AspNetCore.Http;

namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
///     Thrown when a query references an operator that is not registered.
/// </summary>
public sealed class QueryUnknownOperatorException : QueryException
{
    /// <summary>
    ///     Stable <see cref="QueryException.Reason" /> value for all instances of this exception type.
    /// </summary>
    public const string ReasonCode = "QueryUnknownOperator";

    /// <summary>
    ///     Initializes a new instance with HTTP 400.
    /// </summary>
    /// <param name="operatorName">The unknown operator name.</param>
    /// <param name="fieldPath">Optional field path context.</param>
    public QueryUnknownOperatorException(string operatorName, string? fieldPath = null)
        : base(StatusCodes.Status400BadRequest, ReasonCode,
            fieldPath is null
                ? $"Unknown operator '{operatorName}'."
                : $"Unknown operator '{operatorName}' for field '{fieldPath}'.")
    {
        OperatorName = operatorName;
        FieldPath = fieldPath;
    }

    /// <summary>
    ///     Gets the unknown operator name.
    /// </summary>
    public string OperatorName { get; }

    /// <summary>
    ///     Gets the optional field path context.
    /// </summary>
    public string? FieldPath { get; }

    /// <inheritdoc />
    protected override void WriteExtensions(IDictionary<string, object?> extensions)
    {
        extensions["operatorName"] = OperatorName;
        if (FieldPath is not null)
            extensions["fieldPath"] = FieldPath;
    }
}
