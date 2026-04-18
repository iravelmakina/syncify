namespace Syncify.Notifications.Api.Persistence.Entities;

public class Notification
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public Guid UserId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; } = false;
}