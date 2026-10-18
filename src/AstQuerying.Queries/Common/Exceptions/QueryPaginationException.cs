using Microsoft.AspNetCore.Http;

namespace AstQuerying.Queries.Common.Exceptions;

/// <summary>
///     Thrown when pagination parameters violate configured limits or constraints.
/// </summary>
public sealed class QueryPaginationException : QueryException
{
    /// <summary>
    ///     Stable <see cref="QueryException.Reason" /> value for all instances of this exception type.
    /// </summary>
    public const string ReasonCode = "QueryPagination";

    /// <summary>
    ///     Initializes a new instance with HTTP 400.
    /// </summary>
    /// <param name="violation">The pagination violation category.</param>
    /// <param name="message">Human-readable description.</param>
    public QueryPaginationException(PaginationViolation violation, string message)
        : base(StatusCodes.Status400BadRequest, ReasonCode, message)
    {
        Violation = violation;
    }

    /// <summary>
    ///     Gets the pagination violation category.
    /// </summary>
    public PaginationViolation Violation { get; }

    /// <inheritdoc />
    protected override void WriteExtensions(IDictionary<string, object?> extensions)
    {
        extensions["violation"] = Violation.ToString();
    }
}

/// <summary>
///     Categorizes pagination validation failures.
/// </summary>
public enum PaginationViolation
{
    /// <summary>
    ///     The page number is less than 1.
    /// </summary>
    PageTooSmall,

    /// <summary>
    ///     The page size is less than 1.
    /// </summary>
    SizeTooSmall,

    /// <summary>
    ///     The page size exceeds the configured maximum.
    /// </summary>
    SizeExceeded,

    /// <summary>
    ///     The derived skip exceeds the configured maximum.
    /// </summary>
    SkipExceeded
}
