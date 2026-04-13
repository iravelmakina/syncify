using Syncify.Sync.Application.Ports;

namespace Syncify.Sync.Api.Endpoints;

public static class HealthEndpoints
{
    public static RouteGroupBuilder MapHealthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/").WithTags("Health");

        group.MapGet("/health", async (
            ISyncHealthCheck syncHealth,
            CancellationToken ct) =>
        {
            try
            {
                var healthy = await syncHealth.IsHealthyAsync(ct);
                return healthy
                    ? Results.Ok(new { status = "healthy" })
                    : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
            catch
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        });

        return group;
    }
}
