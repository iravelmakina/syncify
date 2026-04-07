using MediatR;
using Syncify.Shared;

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

        var type = result.GetType();
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Result<>))
            return result;

        var isSuccess = (bool)type.GetProperty(nameof(Result<object>.IsSuccess))!.GetValue(result)!;

        if (isSuccess)
        {
            var value = type.GetProperty(nameof(Result<object>.Value))!.GetValue(result);

            if (value is Unit)
                return Results.NoContent();

            return Results.Ok(value);
        }

        var error = (ApplicationError)type.GetProperty(nameof(Result<object>.Error))!.GetValue(result)!;

        return error switch
        {
            ApplicationError.NotFound e => Results.NotFound(new { error = $"{e.Resource} '{e.Id}' not found." }),
            ApplicationError.Validation e => Results.UnprocessableEntity(new { errors = e.Errors }),
            ApplicationError.Conflict e => Results.Conflict(new { error = e.Message }),
            ApplicationError.Forbidden e => Results.Json(new { error = e.Message }, statusCode: StatusCodes.Status403Forbidden),
            _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}