using System.Diagnostics;
using MediatR;

namespace Syncify.Api.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Handling {Request}", requestName);

        var stopwatch = Stopwatch.StartNew();
        var response = await next(cancellationToken);
        stopwatch.Stop();

        logger.LogInformation("Handled {Request} in {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);

        return response;
    }
}