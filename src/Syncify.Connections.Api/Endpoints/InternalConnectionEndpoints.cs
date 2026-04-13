using Syncify.Connections.Api.Responses;
using Syncify.Shared.Ports;

namespace Syncify.Connections.Api.Endpoints;

public static class InternalConnectionEndpoints
{
    public static RouteGroupBuilder MapInternalConnectionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/internal")
            .WithTags("Internal Connections");

        group.MapGet("/calendars/{calendarId:guid}/access", async (
            Guid calendarId,
            IConnectionService connectionService,
            CancellationToken ct) =>
        {
            try
            {
                var access = await connectionService.GetCalendarAccessAsync(calendarId, ct);
                return Results.Ok(new CalendarAccessResponse(access.ToString()));
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        group.MapGet("/calendars/{calendarId:guid}/fresh-token", async (
            Guid calendarId,
            IConnectionService connectionService,
            CancellationToken ct) =>
        {
            try
            {
                var token = await connectionService.GetFreshAccessTokenAsync(calendarId, ct);
                return Results.Ok(new FreshTokenResponse(token));
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        return group;
    }
}
