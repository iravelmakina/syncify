using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Syncify.Shared.Errors;

namespace Syncify.Shared.Middleware;

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
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Invalid request payload."),
            JsonException => (StatusCodes.Status400BadRequest, "Invalid request payload."),
            RequestValidationException => (StatusCodes.Status400BadRequest, "Invalid request payload."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        httpContext.Response.StatusCode = statusCode;
        if (exception is RequestValidationException validationException)
        {
            await httpContext.Response.WriteAsJsonAsync(new
            {
                errors = validationException.Errors
            }, cancellationToken);

            return true;
        }

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode == StatusCodes.Status400BadRequest ? "Bad Request" : exception.GetType().Name,
            Detail = detail
        }, cancellationToken);

        return true;
    }
}
