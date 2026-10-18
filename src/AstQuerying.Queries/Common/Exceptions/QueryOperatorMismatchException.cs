using Microsoft.AspNetCore.Http;

namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
///     Thrown when an operator is not compatible with the field type or comparison shape.
/// </summary>
public sealed class QueryOperatorMismatchException : QueryException
{
    /// <summary>
    ///     Stable <see cref="QueryException.Reason" /> value for all instances of this exception type.
    /// </summary>
    public const string ReasonCode = "QueryOperatorMismatch";

    /// <summary>
    ///     Initializes a new instance with HTTP 400.
    /// </summary>
    /// <param name="operatorName">The operator name.</param>
    /// <param name="fieldPath">The field path.</param>
    /// <param name="mismatchKind">The kind of mismatch.</param>
    /// <param name="message">Human-readable description.</param>
    /// <param name="leafType">Optional leaf CLR type.</param>
    public QueryOperatorMismatchException(
        string operatorName,
        string fieldPath,
        OperatorMismatchKind mismatchKind,
        string message,
        Type? leafType = null)
        : base(StatusCodes.Status400BadRequest, ReasonCode, message)
    {
        OperatorName = operatorName;
        FieldPath = fieldPath;
        MismatchKind = mismatchKind;
        LeafType = leafType;
    }

    /// <summary>
    ///     Gets the operator name.
    /// </summary>
    public string OperatorName { get; }

    /// <summary>
    ///     Gets the field path.
    /// </summary>
    public string FieldPath { get; }

    /// <summary>
    ///     Gets the mismatch category.
    /// </summary>
    public OperatorMismatchKind MismatchKind { get; }

    /// <summary>
    ///     Gets the optional leaf CLR type.
    /// </summary>
    public Type? LeafType { get; }

    /// <inheritdoc />
    protected override void WriteExtensions(IDictionary<string, object?> extensions)
    {
        extensions["operatorName"] = OperatorName;
        extensions["fieldPath"] = FieldPath;
        extensions["mismatchKind"] = MismatchKind.ToString();
        if (LeafType is not null)
            extensions["leafType"] = LeafType.FullName ?? LeafType.Name;
    }
}

/// <summary>
///     Categorizes operator compatibility failures.
/// </summary>
public enum OperatorMismatchKind
{
    /// <summary>
    ///     The operator requires a single value.
    /// </summary>
    RequiresValue,

    /// <summary>
    ///     The operator requires multiple values.
    /// </summary>
    RequiresMultipleValues,

    /// <summary>
    ///     Single-value operators cannot be used for this comparison.
    /// </summary>
    SingleValueNotSupported,

    /// <summary>
    ///     Text operators require a string field.
    /// </summary>
    TextOperatorRequiresString,

    /// <summary>
    ///     Member-only operators cannot be used on value types.
    /// </summary>
    MemberOnlyOnValueType,

    /// <summary>
    ///     The operator is not supported for this comparison.
    /// </summary>
    NotSupported
}
