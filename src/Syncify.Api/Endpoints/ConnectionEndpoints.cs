using MediatR;
using Syncify.Api.Filters;
using Syncify.Api.Middleware;
using Syncify.Connections.Application.Commands.CompleteOAuth;
using Syncify.Connections.Application.Commands.RevokeConnection;
using Syncify.Connections.Application.Queries.GenerateAuthUrl;
using Syncify.Connections.Application.Queries.ListCalendars;
using Syncify.Connections.Application.Queries.ListConnections;

namespace Syncify.Api.Endpoints;

public static class ConnectionEndpoints
{
    public static RouteGroupBuilder MapConnectionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/connections")
            .WithTags("Connections")
            .AddEndpointFilter<ResultEndpointFilter>();

        group.MapPost("/google/auth-url", async (ISender mediator, CancellationToken ct) =>
            await mediator.Send(new GenerateAuthUrlQuery(), ct));

        group.MapPost("/google/callback", async (
            CompleteOAuthRequest request,
            HttpContext context,
            ISender mediator,
            CancellationToken ct) =>
            await mediator.Send(new CompleteOAuthCommand(context.GetUserId(), request.Code), ct));

        group.MapGet("/", async (HttpContext context, ISender mediator, CancellationToken ct) =>
            await mediator.Send(new ListConnectionsQuery(context.GetUserId()), ct));

        group.MapGet("/{accountId:guid}/calendars", async (
            Guid accountId,
            ISender mediator,
            CancellationToken ct) =>
            await mediator.Send(new ListCalendarsQuery(accountId), ct));

        group.MapDelete("/{accountId:guid}", async (
            Guid accountId,
            ISender mediator,
            CancellationToken ct) =>
            await mediator.Send(new RevokeConnectionCommand(accountId), ct));

        return group;
    }
}

public sealed record CompleteOAuthRequest(string Code);