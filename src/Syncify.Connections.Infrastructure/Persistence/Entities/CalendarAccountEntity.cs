namespace Syncify.Connections.Infrastructure.Persistence.Entities;

internal sealed class CalendarAccountEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string RefreshTokenEnc { get; set; } = string.Empty;
    public DateTimeOffset TokenExpiresAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public List<CalendarEntity> Calendars { get; set; } = [];
}
