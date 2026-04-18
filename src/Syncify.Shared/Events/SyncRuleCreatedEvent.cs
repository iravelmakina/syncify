namespace Syncify.Shared.Events;

public record SyncRuleCreatedEvent
{
    public Guid EventId { get; init; }
    public DateTime OccurredAt { get; init; }
    public string? CorrelationId { get; init; }
    public Guid SyncRuleId { get; init; }
    public Guid UserId { get; init; }
    public string Summary { get; init; } = string.Empty;
}
