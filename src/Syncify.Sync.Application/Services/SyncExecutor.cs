using MediatR;
using Syncify.Shared;
using Syncify.Sync.Application.DTOs;
using Syncify.Sync.Application.Ports;
using Syncify.Sync.Domain.Aggregates;
using Syncify.Sync.Domain.Enums;

namespace Syncify.Sync.Application.Services;

public sealed class SyncExecutor(
    ISyncRuleRepository ruleRepository,
    IConnectionService connectionService,
    ICalendarSyncer calendarSyncer,
    ISyncedEventRepository syncedEventRepository)
{
    public async Task<Result<Unit>> ExecuteRuleAsync(Guid ruleId, CancellationToken ct = default)
    {
        var rule = await ruleRepository.GetByIdAsync(ruleId, ct);
        if (rule is null)
            return Result<Unit>.Failure(new ApplicationError.NotFound("SyncRule", ruleId));

        if (rule.Status != SyncRuleStatus.Active)
            return Result<Unit>.Failure(new ApplicationError.Validation(["Only active rules can be executed."]));

        var sourceToken = await connectionService.GetFreshAccessTokenAsync(rule.SourceCalendarId, ct);
        var targetToken = await connectionService.GetFreshAccessTokenAsync(rule.TargetCalendarId, ct);

        var result = await calendarSyncer.FetchChangesAsync(
            rule.SourceCalendarId, sourceToken, rule.SyncCursor, ct);

        foreach (var eventId in result.DeletedEventIds)
            await ProcessDeletionAsync(rule, targetToken, eventId, ct);

        foreach (var ev in result.ChangedEvents)
            await ProcessChangedEventAsync(rule, targetToken, ev, ct);

        rule.UpdateSyncCursor(result.NewCursor, DateTime.UtcNow);
        await ruleRepository.UpdateAsync(rule, ct);

        return Result<Unit>.Success(Unit.Value);
    }

    private async Task ProcessDeletionAsync(
        SyncRule rule, string targetToken, string sourceEventId, CancellationToken ct)
    {
        var mapping = await syncedEventRepository.GetByRuleAndSourceEventAsync(rule.Id, sourceEventId, ct);
        if (mapping is null) return;

        await calendarSyncer.DeleteBlockAsync(rule.TargetCalendarId, targetToken, mapping.TargetBlockId, ct);
        await syncedEventRepository.DeleteAsync(mapping.Id, ct);
    }

    private async Task ProcessChangedEventAsync(
        SyncRule rule, string targetToken, CalendarEventDto ev, CancellationToken ct)
    {
        var title = rule.CopyTitle && ev.Title is not null ? ev.Title : rule.CustomTitle;
        var mapping = await syncedEventRepository.GetByRuleAndSourceEventAsync(rule.Id, ev.Id, ct);

        if (mapping is not null)
        {
            if (ev.UpdatedAt > mapping.SourceUpdatedAt)
            {
                await calendarSyncer.UpdateBlockAsync(
                    rule.TargetCalendarId, targetToken, mapping.TargetBlockId,
                    title, ev.Start, ev.End, ct);

                await syncedEventRepository.UpdateAsync(mapping with { SourceUpdatedAt = ev.UpdatedAt }, ct);
            }
        }
        else
        {
            var blockId = await calendarSyncer.CreateBlockAsync(
                rule.TargetCalendarId, targetToken,
                title, ev.Start, ev.End, ct);

            await syncedEventRepository.CreateAsync(
                new SyncedEventMapping(Guid.NewGuid(), rule.Id, ev.Id, blockId, ev.UpdatedAt), ct);
        }
    }
}
