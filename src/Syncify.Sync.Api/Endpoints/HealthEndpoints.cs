using Syncify.Connections.Application.Ports;
using Syncify.Sync.Application.Ports;

namespace Syncify.Sync.Api.Endpoints;

public static class HealthEndpoints
{
    public static RouteGroupBuilder MapHealthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/").WithTags("Health");

        group.MapGet("/health", async (
            IConnectionsHealthCheck connectionsHealth,
            ISyncHealthCheck syncHealth,
            CancellationToken ct) =>
        {
            try
            {
                var connectionsOk = await connectionsHealth.IsHealthyAsync(ct);
                var syncOk = await syncHealth.IsHealthyAsync(ct);

                if (connectionsOk && syncOk)
                    return Results.Ok(new { status = "healthy" });

                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
            catch
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        });

        return group;
    }
}
