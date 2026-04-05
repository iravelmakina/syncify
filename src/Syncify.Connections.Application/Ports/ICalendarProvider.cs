using Syncify.Connections.Domain.ValueObjects;

namespace Syncify.Connections.Application.Ports;

public interface ICalendarProvider
{
    Task<IReadOnlyList<CalendarInfo>> ListCalendarsAsync(string accessToken, CancellationToken ct = default);
}