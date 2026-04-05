using MediatR;
using Syncify.Shared;

namespace Syncify.Api.Behaviors;

public sealed class DomainExceptionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (DomainException ex)
        {
            if (typeof(TResponse).IsGenericType
                && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var error = new ApplicationError.Validation([ex.Message]);
                var failureMethod = typeof(TResponse).GetMethod(nameof(Result<object>.Failure))!;
                return (TResponse)failureMethod.Invoke(null, [error])!;
            }

            throw;
        }
    }
}