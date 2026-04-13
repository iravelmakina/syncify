using MediatR;
using Syncify.Api.Mappers;
using Syncify.Shared.Results;

namespace Syncify.Api.Filters;

public sealed class ResultEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var result = await next(context);

        if (result is null)
            return Results.NoContent();

        if (result is not IResultContract operationResult)
            return result;

        if (operationResult.IsSuccess)
        {
            var value = operationResult.ValueObject;

            if (value is Unit)
                return Results.NoContent();

            return Results.Ok(value);
        }

        return ApplicationResultMapper.ToHttpResult(operationResult.Error!);
    }
}
