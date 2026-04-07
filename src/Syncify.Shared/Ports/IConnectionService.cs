using Syncify.Shared.Enums;

namespace Syncify.Shared.Ports;

public interface IConnectionService
{
    Task<CalendarAccess> GetCalendarAccessAsync(Guid calendarId, CancellationToken ct = default);
    Task<string> GetFreshAccessTokenAsync(Guid calendarId, CancellationToken ct = default);
    Task<string> GetProviderCalendarIdAsync(Guid calendarId, CancellationToken ct = default);
}
