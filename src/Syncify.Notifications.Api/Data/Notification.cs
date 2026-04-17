namespace Syncify.Notifications.Api.Data;

public class Notification
{
    public Guid EventId { get; set; }
    public string? CorrelationId { get; set; }
    public Guid SyncRuleId { get; set; }
    public Guid UserId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}