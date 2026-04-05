namespace Syncify.Shared;

public interface IConnectionService
{
    Task<CalendarAccess> GetCalendarAccessAsync(Guid calendarId, CancellationToken ct = default);
    Task<string> GetFreshAccessTokenAsync(Guid calendarId, CancellationToken ct = default);
}