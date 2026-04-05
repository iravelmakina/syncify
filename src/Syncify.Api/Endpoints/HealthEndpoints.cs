using Microsoft.EntityFrameworkCore;
using Syncify.Connections.Infrastructure.Persistence;
using Syncify.Sync.Infrastructure.Persistence;

namespace Syncify.Api.Endpoints;

public static class HealthEndpoints
{
    public static RouteGroupBuilder MapHealthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/").WithTags("Health");

        group.MapGet("/health", async (
            ConnectionsDbContext connectionsDb,
            SyncDbContext syncDb,
            CancellationToken ct) =>
        {
            try
            {
                await connectionsDb.Database.CanConnectAsync(ct);
                await syncDb.Database.CanConnectAsync(ct);
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