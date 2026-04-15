using Syncify.Connections.Application.Ports;

namespace Syncify.Connections.Api.Endpoints;

public static class HealthEndpoints
{
    public static RouteGroupBuilder MapHealthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/").WithTags("Health");

        group.MapGet("/health", async (
            IConnectionsHealthCheck connectionsHealth,
            CancellationToken ct) =>
        {
            try
            {
                var healthy = await connectionsHealth.IsHealthyAsync(ct);
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
