using System.Text.Json;
using AstQuerying.Queries.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace AstQuerying.Queries.Handlers;

/// <summary>
/// ASP.NET Core <see cref="IExceptionHandler"/> that maps <see cref="QueryException"/> to RFC 7807 <see cref="ProblemDetails"/> using <see cref="IProblemDetailsService"/>.
/// </summary>
public sealed class QueryExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not QueryException queryException)
            return false;

        var statusCode = queryException.StatusCode;
        httpContext.Response.StatusCode = statusCode;

        var title = ReasonPhrases.GetReasonPhrase(statusCode);
        if (string.IsNullOrEmpty(title))
            title = "Error";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = queryException.Message
        };

        problemDetails.Extensions["exceptionType"] = queryException.GetType().FullName ?? queryException.GetType().Name;
        problemDetails.Extensions["reason"] = queryException.Reason;

        if (queryException is QueryConfigurationException configurationException &&
            configurationException.Diagnostics is not null)
        {
            problemDetails.Extensions["configuration"] =
                JsonSerializer.SerializeToElement(configurationException.Diagnostics);
        }

        if (queryException.InnerException is not null)
        {
            problemDetails.Extensions["innerExceptionType"] =
                queryException.InnerException.GetType().FullName ?? queryException.InnerException.GetType().Name;
            problemDetails.Extensions["innerExceptionMessage"] = queryException.InnerException.Message;
        }

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = queryException
        }).ConfigureAwait(false);

        return true;
    }
}
