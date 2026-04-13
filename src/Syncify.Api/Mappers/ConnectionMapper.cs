using Syncify.Api.Responses;
using Syncify.Connections.Application.Queries.ListConnections;
using Syncify.Connections.Domain.ValueObjects;

namespace Syncify.Api.Mappers;

internal static class ConnectionMapper
{
    public static ConnectionResponse ToResponse(this ConnectionListItem item) =>
        new(item.Id, item.Provider, item.Email, item.Status, item.CreatedAt);

    public static IReadOnlyList<ConnectionResponse> ToResponse(this IReadOnlyList<ConnectionListItem> items) =>
        items.Select(ToResponse).ToList();

    public static CalendarResponse ToResponse(this CalendarInfo calendar) =>
        new(calendar.Id, calendar.ProviderCalendarId, calendar.Name, calendar.Access.ToString());

    public static IReadOnlyList<CalendarResponse> ToResponse(this IReadOnlyList<CalendarInfo> calendars) =>
        calendars.Select(ToResponse).ToList();
}