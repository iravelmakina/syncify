using MediatR;
using Syncify.Shared.Errors;
using Syncify.Shared.Results;

namespace Syncify.Shared.Behaviors;

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
                ApplicationError error = ex.Code switch
                {
                    DomainErrorCode.InvalidState => new ApplicationError.Conflict(ex.Message),
                    DomainErrorCode.AccessViolation => new ApplicationError.Forbidden(ex.Message),
                    _ => new ApplicationError.Validation([ex.Message])
                };
                var failureMethod = typeof(TResponse).GetMethod(nameof(Result<object>.Failure))!;
                return (TResponse)failureMethod.Invoke(null, [error])!;
            }

            throw;
        }
    }
}
