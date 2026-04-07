
using Syncify.Shared.Enums;
using Syncify.Shared.Errors;
using Syncify.Shared.Ports;

namespace Syncify.Api.Mappers;

internal static class ApplicationResultMapper
{
    public static IResult ToHttpResult(ApplicationError error)
    {
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