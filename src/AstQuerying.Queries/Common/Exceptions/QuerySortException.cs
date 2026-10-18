using Microsoft.AspNetCore.Http;

namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
///     Thrown when sort parameters are invalid.
/// </summary>
public sealed class QuerySortException : QueryException
{
    /// <summary>
    ///     Stable <see cref="QueryException.Reason" /> value for all instances of this exception type.
    /// </summary>
    public const string ReasonCode = "QuerySort";

    /// <summary>
    ///     Initializes a new instance with HTTP 400.
    /// </summary>
    /// <param name="violation">The sort violation category.</param>
    /// <param name="message">Human-readable description.</param>
    public QuerySortException(SortViolation violation, string message)
        : base(StatusCodes.Status400BadRequest, ReasonCode, message)
    {
        Violation = violation;
    }

    /// <summary>
    ///     Gets the sort violation category.
    /// </summary>
    public SortViolation Violation { get; }

    /// <inheritdoc />
    protected override void WriteExtensions(IDictionary<string, object?> extensions)
    {
        extensions["violation"] = Violation.ToString();
    }
}

/// <summary>
///     Categorizes sort validation failures.
/// </summary>
public enum SortViolation
{
    /// <summary>
    ///     The sort list is empty.
    /// </summary>
    EmptySortList
}
