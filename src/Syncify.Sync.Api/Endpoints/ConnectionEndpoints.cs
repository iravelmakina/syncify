using MediatR;
using Syncify.Sync.Api.Filters;
using Syncify.Sync.Api.Mappers;
using Syncify.Sync.Api.Middleware;
using Syncify.Sync.Api.Responses;
using Syncify.Connections.Application.Commands.CompleteOAuth;
using Syncify.Connections.Application.Commands.RevokeConnection;
using Syncify.Connections.Application.Queries.GenerateAuthUrl;
using Syncify.Connections.Application.Queries.ListCalendars;
using Syncify.Connections.Application.Queries.ListConnections;

namespace Syncify.Sync.Api.Endpoints;

public static class ConnectionEndpoints
{
    public static RouteGroupBuilder MapConnectionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/connections")
            .WithTags("Connections")
            .AddEndpointFilter<ResultEndpointFilter>();

        group.MapPost("/google/auth-url", async (ISender mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GenerateAuthUrlQuery(), ct);
            return result.Map(url => new AuthUrlResponse(url));
        });

        group.MapPost("/google/callback", async (
            CompleteOAuthRequest request,
            HttpContext context,
            ISender mediator,
            CancellationToken ct) =>
            await mediator.Send(new CompleteOAuthCommand(context.GetUserId(), request.Code), ct));

        group.MapGet("/", async (HttpContext context, ISender mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListConnectionsQuery(context.GetUserId()), ct);
            return result.Map(items => items.ToResponse());
        });

        group.MapGet("/{accountId:guid}/calendars", async (
            Guid accountId,
            ISender mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListCalendarsQuery(accountId), ct);
            return result.Map(calendars => calendars.ToResponse());
        });

        group.MapDelete("/{accountId:guid}", async (
            Guid accountId,
            ISender mediator,
            CancellationToken ct) =>
            await mediator.Send(new RevokeConnectionCommand(accountId), ct));

        return group;
    }
}

public sealed record CompleteOAuthRequest(string Code);