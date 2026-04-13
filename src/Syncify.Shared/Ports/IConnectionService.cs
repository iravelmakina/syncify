using Syncify.Shared.Enums;

namespace Syncify.Shared.Ports;

public sealed record ProviderCalendarAccessToken(string AccessToken, string ProviderCalendarId);

public interface IConnectionService
{
    Task<CalendarAccess> GetCalendarAccessAsync(Guid calendarId, CancellationToken ct = default);
    Task<ProviderCalendarAccessToken> GetProviderCalendarAccessTokenAsync(Guid calendarId, CancellationToken ct = default);
}
