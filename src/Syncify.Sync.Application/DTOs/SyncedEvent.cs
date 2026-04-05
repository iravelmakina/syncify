namespace Syncify.Sync.Application.DTOs;

public sealed record SyncedEvent(
    Guid Id,
    Guid SyncRuleId,
    string SourceEventId,
    string TargetBlockId,
    DateTime SourceUpdatedAt);