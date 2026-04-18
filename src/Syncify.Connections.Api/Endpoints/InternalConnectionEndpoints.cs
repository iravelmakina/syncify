using Syncify.Connections.Api.Responses;
using Syncify.Shared.Contracts;
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
                var access = await connectionService.GetCalendarAccessAsync(calendarId, null, ct);
                return Results.Ok(new CalendarAccessResponse(access.ToString()));
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        group.MapGet("/calendars/{calendarId:guid}/token", async (
            Guid calendarId,
            IConnectionService connectionService,
            CancellationToken ct) =>
        {
            try
            {
                var result = await connectionService.GetProviderCalendarAccessTokenAsync(calendarId, null, ct);
                return Results.Ok(new ProviderCalendarAccessTokenResponse(result.AccessToken, result.ProviderCalendarId));
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        return group;
    }
}
