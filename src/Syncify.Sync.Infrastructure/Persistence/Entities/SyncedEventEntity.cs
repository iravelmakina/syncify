namespace Syncify.Sync.Infrastructure.Persistence.Entities;

public class SyncedEventEntity
{
    public Guid Id { get; set; }
    public Guid SyncRuleId { get; set; }
    public string SourceEventId { get; set; } = string.Empty;
    public string TargetBlockId { get; set; } = string.Empty;
    public DateTime SourceUpdatedAt { get; set; }

    public SyncRuleEntity SyncRule { get; set; } = null!;
}
