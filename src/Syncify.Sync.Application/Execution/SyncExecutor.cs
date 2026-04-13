using MediatR;
using Microsoft.Extensions.Logging;
using Syncify.Shared.Errors;
using Syncify.Shared.Ports;
using Syncify.Shared.Results;
using Syncify.Sync.Application.Models;
using Syncify.Sync.Application.Ports;
using Syncify.Sync.Domain.Aggregates;
using Syncify.Sync.Domain.Enums;
using Syncify.Sync.Domain.ValueObjects;

namespace Syncify.Sync.Application.Execution;

public sealed class SyncExecutor(
    ISyncRuleRepository ruleRepository,
    IConnectionService connectionService,
    ICalendarSyncer calendarSyncer,
    ISyncedEventRepository syncedEventRepository,
    ILogger<SyncExecutor> logger) : ISyncExecutor
{
    public async Task<Result<Unit>> ExecuteRuleAsync(Guid ruleId, CancellationToken ct = default)
    {
        logger.LogInformation("Starting sync execution for rule {RuleId}", ruleId);

        var rule = await ruleRepository.GetByIdAsync(ruleId, ct);
        if (rule is null)
        {
            logger.LogWarning("SyncRule {RuleId} not found", ruleId);
            return Result<Unit>.Failure(new ApplicationError.NotFound("SyncRule", ruleId));
        }

        try
        {
            var targetToken = await connectionService.GetFreshAccessTokenAsync(rule.TargetCalendarId, ct);
            var targetProviderCalendarId = await connectionService.GetProviderCalendarIdAsync(rule.TargetCalendarId, ct);

            if (rule.SyncCursor is null)
            {
                await CleanupFutureBlocksAsync(rule, targetProviderCalendarId, targetToken, ct);
            }

            if (rule.Status != SyncRuleStatus.Active)
            {
                logger.LogInformation("Sync skipped for {RuleId} with status {Status}", ruleId, rule.Status);
                return Result<Unit>.Success(Unit.Value);
            }

            var sourceToken = await connectionService.GetFreshAccessTokenAsync(rule.SourceCalendarId, ct);
            var sourceProviderCalendarId = await connectionService.GetProviderCalendarIdAsync(rule.SourceCalendarId, ct);

            var result = await calendarSyncer.FetchChangesAsync(
                sourceProviderCalendarId, sourceToken, rule.SyncCursor, rule.LookbackDays, ct);

            logger.LogInformation("Fetched changes for rule {RuleId}: {ChangedCount} changed, {DeletedCount} deleted", ruleId, result.ChangedEvents.Count, result.DeletedEventIds.Count);

            foreach (var eventId in result.DeletedEventIds)
                await ProcessDeletionAsync(rule, targetProviderCalendarId, targetToken, eventId, ct);

            foreach (var ev in result.ChangedEvents)
                await ProcessChangedEventAsync(rule, targetProviderCalendarId, targetToken, ev, result.TimeZone, ct);

            rule.UpdateSyncCursor(result.NewCursor, DateTime.UtcNow);
            await ruleRepository.UpdateAsync(rule, ct);

            logger.LogInformation("Successfully completed sync execution for rule {RuleId}", ruleId);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during sync execution for rule {RuleId}", ruleId);
            throw;
        }
    }

    private async Task CleanupFutureBlocksAsync(
        SyncRule rule,
        string targetProviderCalendarId,
        string targetToken,
        CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow;

        var staleMappings = await syncedEventRepository.ListByRuleSinceAsync(rule.Id, cutoff, ct);
        if (staleMappings.Count == 0) return;

        logger.LogInformation(
            "Cleaning up {Count} future blocks for rule {RuleId} (cutoff: {Cutoff})",
            staleMappings.Count, rule.Id, cutoff);

        foreach (var mapping in staleMappings)
        {
            await calendarSyncer.DeleteBlockAsync(
                targetProviderCalendarId, targetToken, mapping.TargetBlockId, ct);
        }

        await syncedEventRepository.DeleteByRuleSinceAsync(rule.Id, cutoff, ct);
    }

    private async Task ProcessDeletionAsync(
        SyncRule rule,
        string targetProviderCalendarId,
        string targetToken,
        string sourceEventId,
        CancellationToken ct)
    {
        var mapping = await syncedEventRepository.GetByRuleAndSourceEventAsync(rule.Id, sourceEventId, ct);
        if (mapping is null) return;

        await calendarSyncer.DeleteBlockAsync(targetProviderCalendarId, targetToken, mapping.TargetBlockId, ct);
        await syncedEventRepository.DeleteAsync(mapping.Id, ct);
    }

    private async Task ProcessChangedEventAsync(
        SyncRule rule,
        string targetProviderCalendarId,
        string targetToken,
        CalendarEventDto ev,
        string? sourceTimeZone,
        CancellationToken ct)
    {
        var mapping = await syncedEventRepository.GetByRuleAndSourceEventAsync(rule.Id, ev.Id, ct);

        if (!rule.FilterPolicy.Matches(new EventSnapshot(ev.Title, ev.Start, ev.End, sourceTimeZone)))
        {
            
            if (mapping is not null)
            {
                await calendarSyncer.DeleteBlockAsync(
                    targetProviderCalendarId, targetToken, mapping.TargetBlockId, ct);
                await syncedEventRepository.DeleteAsync(mapping.Id, ct);
            }

            return;
        }

        var title = rule.CopyTitle && ev.Title is not null ? ev.Title : rule.CustomTitle;

        if (mapping is not null)
        {
            if (ev.UpdatedAt > mapping.SourceUpdatedAt)
            {
                await calendarSyncer.UpdateBlockAsync(
                    targetProviderCalendarId, targetToken, mapping.TargetBlockId,
                    title, ev.Start, ev.End, ct);

                await syncedEventRepository.UpdateAsync(
                    mapping with { SourceStart = ev.Start, SourceUpdatedAt = ev.UpdatedAt }, ct);
            }
        }
        else
        {
            var blockId = await calendarSyncer.CreateBlockAsync(
                targetProviderCalendarId, targetToken,
                title, ev.Start, ev.End, ct);

            await syncedEventRepository.CreateAsync(
                new SyncedEventMapping(Guid.NewGuid(), rule.Id, ev.Id, blockId, ev.Start, ev.UpdatedAt), ct);
        }
    }
}
