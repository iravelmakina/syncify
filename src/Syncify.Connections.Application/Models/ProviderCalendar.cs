using Syncify.Shared.Enums;

namespace Syncify.Connections.Application.Models;

public sealed record ProviderCalendar(string ProviderCalendarId, string Name, CalendarAccess Access);
