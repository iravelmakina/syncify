using MediatR;
using Syncify.Api.Filters;
using Syncify.Api.Mappers;
using Syncify.Api.Middleware;
using Syncify.Api.Requests;
using Syncify.Sync.Application.Commands.ArchiveSyncRule;
using Syncify.Sync.Application.Commands.CreateSyncRule;
using Syncify.Sync.Application.Commands.ExecuteSyncRule;
using Syncify.Sync.Application.Commands.ResumeSyncRule;
using Syncify.Sync.Application.Commands.UpdateFilter;
using Syncify.Sync.Application.Commands.UpdateTitle;
using Syncify.Sync.Application.Queries.GetSyncRule;
using Syncify.Sync.Application.Queries.ListSyncRules;

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
                request.FilterPolicy.ToDomain()), ct));

        group.MapGet("/{id:guid}", async (Guid id, ISender mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetSyncRuleQuery(id), ct);
            return result.Map(rule => rule.ToResponse());
        });

        group.MapGet("/", async (HttpContext context, ISender mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListSyncRulesQuery(context.GetUserId()), ct);
            return result.Map(rules => rules.ToResponse());
        });

        group.MapPost("/{id:guid}/archive", async (Guid id, ISender mediator, CancellationToken ct) =>
            await mediator.Send(new ArchiveSyncRuleCommand(id), ct));

        group.MapPost("/{id:guid}/resume", async (Guid id, ISender mediator, CancellationToken ct) =>
            await mediator.Send(new ResumeSyncRuleCommand(id), ct));

        group.MapPatch("/{id:guid}/filter", async (
            Guid id,
            UpdateFilterRequest request,
            ISender mediator,
            CancellationToken ct) =>
            await mediator.Send(new UpdateFilterCommand(id, request.FilterPolicy.ToDomain()), ct));

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