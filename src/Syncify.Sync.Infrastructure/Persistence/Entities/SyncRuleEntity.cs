namespace Syncify.Sync.Infrastructure.Persistence.Entities;

internal sealed class SyncRuleEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SourceCalendarId { get; set; }
    public Guid TargetCalendarId { get; set; }
    public bool CopyTitle { get; set; }
    public string CustomTitle { get; set; } = string.Empty;
    public string FilterPolicyJson { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? SyncCursor { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
