using MediatR;
using Syncify.Api.Filters;
using Syncify.Api.Middleware;
using Syncify.Sync.Application.Commands.ArchiveSyncRule;
using Syncify.Sync.Application.Commands.CreateSyncRule;
using Syncify.Sync.Application.Commands.ExecuteSyncRule;
using Syncify.Sync.Application.Commands.ResumeSyncRule;
using Syncify.Sync.Application.Commands.UpdateFilter;
using Syncify.Sync.Application.Commands.UpdateTitle;
using Syncify.Sync.Application.Queries.GetSyncRule;
using Syncify.Sync.Application.Queries.ListSyncRules;
using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Api.Endpoints;

public static class SyncRuleEndpoints
{
    public static RouteGroupBuilder MapSyncRuleEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/sync-rules")
            .WithTags("Sync Rules")
            .AddEndpointFilter<ResultEndpointFilter>();

        group.MapPost("/", async (
            CreateSyncRuleRequest request,
            HttpContext context,
            ISender mediator,
            CancellationToken ct) =>
            await mediator.Send(new CreateSyncRuleCommand(
                context.GetUserId(),
                request.SourceCalendarId,
                request.TargetCalendarId,
                request.CopyTitle,
                request.CustomTitle,
                request.FilterPolicy), ct));

        group.MapGet("/{id:guid}", async (Guid id, ISender mediator, CancellationToken ct) =>
            await mediator.Send(new GetSyncRuleQuery(id), ct));

        group.MapGet("/", async (HttpContext context, ISender mediator, CancellationToken ct) =>
            await mediator.Send(new ListSyncRulesQuery(context.GetUserId()), ct));

        group.MapPost("/{id:guid}/archive", async (Guid id, ISender mediator, CancellationToken ct) =>
            await mediator.Send(new ArchiveSyncRuleCommand(id), ct));

        group.MapPost("/{id:guid}/resume", async (Guid id, ISender mediator, CancellationToken ct) =>
            await mediator.Send(new ResumeSyncRuleCommand(id), ct));

        group.MapPatch("/{id:guid}/filter", async (
            Guid id,
            UpdateFilterRequest request,
            ISender mediator,
            CancellationToken ct) =>
            await mediator.Send(new UpdateFilterCommand(id, request.FilterPolicy), ct));

        group.MapPatch("/{id:guid}/title", async (
            Guid id,
            UpdateTitleRequest request,
            ISender mediator,
            CancellationToken ct) =>
            await mediator.Send(new UpdateTitleCommand(id, request.CopyTitle, request.CustomTitle), ct));

        group.MapPost("/{id:guid}/execute", async (Guid id, ISender mediator, CancellationToken ct) =>
            await mediator.Send(new ExecuteSyncCommand(id), ct));

        return group;
    }
}

public sealed record CreateSyncRuleRequest(
    Guid SourceCalendarId,
    Guid TargetCalendarId,
    bool CopyTitle,
    string CustomTitle,
    FilterPolicy FilterPolicy);

public sealed record UpdateFilterRequest(FilterPolicy FilterPolicy);

public sealed record UpdateTitleRequest(bool CopyTitle, string CustomTitle);