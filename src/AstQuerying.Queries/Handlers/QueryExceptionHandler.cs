using System.Text.Json;
using AstQuerying.Queries.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace AstQuerying.Queries.Handlers;

/// <summary>
///     ASP.NET Core <see cref="IExceptionHandler" /> that maps <see cref="QueryException" /> to RFC 7807
///     <see cref="ProblemDetails" /> using <see cref="IProblemDetailsService" />.
/// </summary>
public sealed class QueryExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not QueryException queryException)
            return false;

        httpContext.Response.StatusCode = queryException.StatusCode;

        var title = ReasonPhrases.GetReasonPhrase(queryException.StatusCode);
        if (string.IsNullOrEmpty(title))
            title = exception.Message;

        var detailsExtensions = new Dictionary<string, object?>
        {
            ["exceptionType"] = queryException.GetType().FullName ?? queryException.GetType().Name,
            ["reason"] = queryException.Reason
        };
        if (queryException is QueryConfigurationException configurationException &&
            configurationException.Diagnostics is not null)
            detailsExtensions["configuration"] =
                JsonSerializer.SerializeToElement(configurationException.Diagnostics);

        var problemDetails = new ProblemDetails
        {
            Status = queryException.StatusCode,
            Title = title,
            Detail = queryException.Message,
            Extensions = detailsExtensions
        };
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = queryException,
        }).ConfigureAwait(false);

        return true;
    }
}