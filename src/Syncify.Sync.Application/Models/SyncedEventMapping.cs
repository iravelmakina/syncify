namespace Syncify.Sync.Application.DTOs;

public sealed record SyncedEventMapping(
    Guid Id,
    Guid SyncRuleId,
    string SourceEventId,
    string TargetBlockId,
    DateTime SourceStart,
    DateTime SourceUpdatedAt);