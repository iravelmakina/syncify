using Syncify.Connections.Application.Models;

namespace Syncify.Connections.Application.Ports;

public interface ICalendarProvider
{
    Task<IReadOnlyList<ProviderCalendar>> ListCalendarsAsync(string accessToken, CancellationToken ct = default);
}