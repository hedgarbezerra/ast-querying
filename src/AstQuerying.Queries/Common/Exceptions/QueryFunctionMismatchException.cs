using Microsoft.AspNetCore.Http;

namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
///     Thrown when a function cannot be applied to the target field type.
/// </summary>
public sealed class QueryFunctionMismatchException : QueryException
{
    /// <summary>
    ///     Stable <see cref="QueryException.Reason" /> value for all instances of this exception type.
    /// </summary>
    public const string ReasonCode = "QueryFunctionMismatch";

    /// <summary>
    ///     Initializes a new instance with HTTP 400.
    /// </summary>
    /// <param name="functionName">The function name.</param>
    /// <param name="fieldPath">The field path.</param>
    /// <param name="leafType">The field leaf CLR type.</param>
    public QueryFunctionMismatchException(string functionName, string fieldPath, Type leafType)
        : base(StatusCodes.Status400BadRequest, ReasonCode,
            $"Function '{functionName}' cannot be applied to field '{fieldPath}' of type {leafType.Name}.")
    {
        FunctionName = functionName;
        FieldPath = fieldPath;
        LeafType = leafType;
    }

    /// <summary>
    ///     Gets the function name.
    /// </summary>
    public string FunctionName { get; }

    /// <summary>
    ///     Gets the field path.
    /// </summary>
    public string FieldPath { get; }

    /// <summary>
    ///     Gets the field leaf CLR type.
    /// </summary>
    public Type LeafType { get; }

    /// <inheritdoc />
    protected override void WriteExtensions(IDictionary<string, object?> extensions)
    {
        extensions["functionName"] = FunctionName;
        extensions["fieldPath"] = FieldPath;
        extensions["leafType"] = LeafType.FullName ?? LeafType.Name;
    }
}
