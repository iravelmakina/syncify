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

        foreach (var change in result.Changes)
            await ProcessChangeAsync(rule, targetToken, change, ct);

        rule.UpdateSyncCursor(result.NewCursor, DateTime.UtcNow);
        await ruleRepository.UpdateAsync(rule, ct);

        return Result<Unit>.Success(Unit.Value);
    }

    private async Task ProcessChangeAsync(
        SyncRule rule, string targetToken, CalendarChange change, CancellationToken ct)
    {
        var mapping = await syncedEventRepository.GetByRuleAndSourceEventAsync(rule.Id, change.EventId, ct);

        if (change.IsCancelled)
        {
            if (mapping is not null)
            {
                await calendarSyncer.DeleteBlockAsync(rule.TargetCalendarId, targetToken, mapping.TargetBlockId, ct);
                await syncedEventRepository.DeleteAsync(mapping.Id, ct);
            }
            return;
        }

        var title = rule.CopyTitle && change.Title is not null ? change.Title : rule.CustomTitle;

        if (mapping is not null)
        {
            if (change.UpdatedAt > mapping.SourceUpdatedAt)
            {
                await calendarSyncer.UpdateBlockAsync(
                    rule.TargetCalendarId, targetToken, mapping.TargetBlockId,
                    title, change.Start!.Value, change.End!.Value, ct);

                var updated = mapping with { SourceUpdatedAt = change.UpdatedAt };
                await syncedEventRepository.UpdateAsync(updated, ct);
            }
        }
        else
        {
            var blockId = await calendarSyncer.CreateBlockAsync(
                rule.TargetCalendarId, targetToken,
                title, change.Start!.Value, change.End!.Value, ct);

            await syncedEventRepository.CreateAsync(
                new SyncedEventMapping(Guid.NewGuid(), rule.Id, change.EventId, blockId, change.UpdatedAt), ct);
        }
    }
}