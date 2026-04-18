using Syncify.Notifications.Api.Persistence;

namespace Syncify.Notifications.Api.Endpoints;

public static class HealthEndpoints
{
    public static RouteGroupBuilder MapHealthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/").WithTags("Health");

        group.MapGet("/health", async (
            NotificationsDbContext dbContext,
            CancellationToken ct) =>
        {
            try
            {
                await dbContext.Database.CanConnectAsync(ct);
                return Results.Ok(new { status = "healthy" });
            }
            catch
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        });

        return group;
    }
}
