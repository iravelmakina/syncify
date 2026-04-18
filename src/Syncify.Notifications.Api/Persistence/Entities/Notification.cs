namespace Syncify.Notifications.Api.Persistence.Entities;

public class Notification
{
    public Guid EventId { get; set; }
    public string? CorrelationId { get; set; }
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
