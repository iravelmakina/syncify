namespace Syncify.Sync.Application.DTOs;

public sealed class SyncedEventMapping
{
    public Guid Id { get; set; }
    public Guid SyncRuleId { get; set; }
    public string SourceEventId { get; set; } = default!;
    public string TargetBlockId { get; set; } = default!;
    public DateTime SourceUpdatedAt { get; set; }
}