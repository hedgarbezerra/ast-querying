using System.Text.Json;
using AstQuerying.Queries.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace AstQuerying.Queries.Common.AspNetCore;

/// <summary>
/// ASP.NET Core <see cref="IExceptionHandler"/> that serializes <see cref="QueryException"/> to JSON problem responses.
/// </summary>
public sealed class QueryExceptionHandler : IExceptionHandler
{
    /// <summary>
    /// Writes a JSON body with status code and reason when <paramref name="exception"/> is a <see cref="QueryException"/>.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="exception">The exception being handled.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when handled; otherwise <see langword="false"/>.</returns>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not QueryException qe)
            return false;

        httpContext.Response.StatusCode = qe.StatusCode;
        httpContext.Response.ContentType = "application/json; charset=utf-8";
        await JsonSerializer.SerializeAsync(httpContext.Response.Body,
            new { statusCode = qe.StatusCode, reason = qe.Reason },
            cancellationToken: cancellationToken);
        return true;
    }
}
