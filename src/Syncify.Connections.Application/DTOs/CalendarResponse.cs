namespace Syncify.Connections.Application.DTOs;

public sealed record CalendarResponse(Guid Id, string ProviderCalendarId, string Name, string Access);