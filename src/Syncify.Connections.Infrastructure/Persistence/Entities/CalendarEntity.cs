namespace Syncify.Connections.Infrastructure.Persistence.Entities;

internal sealed class CalendarEntity
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string ProviderCalendarId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Access { get; set; } = string.Empty;

    public CalendarAccountEntity Account { get; set; } = null!;
}
