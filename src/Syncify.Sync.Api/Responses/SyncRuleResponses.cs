namespace Syncify.Sync.Api.Responses;

public sealed record SyncRuleResponse(
    Guid Id,
    Guid SourceCalendarId,
    Guid TargetCalendarId,
    bool CopyTitle,
    string CustomTitle,
    string Status,
    DateTime CreatedAt);
