using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Syncify.Shared;

namespace Syncify.Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception");

        var (statusCode, detail) = exception switch
        {
            ArgumentException e => (StatusCodes.Status400BadRequest, e.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = exception.GetType().Name,
            Detail = detail
        }, cancellationToken);

        return true;
    }
}
